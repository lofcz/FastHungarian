[![FastHungarian](https://badgen.net/nuget/v/FastHungarian?v=302&icon=nuget&label=FastHungarian)](https://www.nuget.org/packages/FastHungarian)

# FastHungarian

Fast assignment problem solver based on modified [Kwok's algorithm](https://www.arxiv.org/pdf/2502.20889) for maximum weight matching on bipartite graphs. Supports anything from `.NET Standard 2.0` to modern `.NET 10+` with no dependencies. Solves both minimum cost assignment (Hungarian algorithm) and maximum weight matching problems. **~12-20× faster** than classical Hungarian algorithm implementations while maintaining optimal solutions. Extensively tested with comprehensive fuzz testing across thousands of matrix configurations.

## Getting Started

Install the package via NuGet:

```powershell
dotnet add package FastHungarian
```

Solve assignment problems:

```csharp
using FastHungarian;

// Cost matrix: assign workers to tasks minimizing total cost
int[,] costs = new int[,]
{
    { 10, 25, 15, 20 },  // Worker 0 costs for tasks 0-3
    { 15, 30, 5,  15 },  // Worker 1 costs for tasks 0-3
    { 20, 5,  10, 25 },  // Worker 2 costs for tasks 0-3
    { 25, 20, 15, 10 }   // Worker 3 costs for tasks 0-3
};

int[] assignments = FastHungarian.FindAssignments(costs);
// assignments[i] = task assigned to worker i
// Result: [2, 2, 1, 3] means Worker 0→Task 2, Worker 1→Task 2, etc.
```

⭐ **That's it!** The algorithm automatically handles:
- Non-square matrices (different number of workers and tasks)
- Transposition for optimal performance
- Edge retention optimization for large graphs

## Advanced Usage

### Get Total Cost

```csharp
Matching result = FastHungarian.FindAssignmentsWithWeight(costs);
Console.WriteLine($"Optimal cost: {result.WeightSum}");
Console.WriteLine($"Assignments: {string.Join(", ", result.LeftPairs)}");
```

### Maximum Weight Matching (Adjacency List API)

For sparse graphs or when you want to maximize weights instead of minimize costs:

```csharp
// Bipartite graph: L = {0, 1, 2}, R = {0, 1, 2, 3}
// Each list contains (vertex, weight) pairs
var adjacencyList = new List<List<(int vertex, int weight)>>
{
    [(0, 10), (1, 5), (2, 3)],     // Vertex 0 edges
    [(1, 8), (2, 7), (3, 9)],      // Vertex 1 edges
    [(0, 4), (2, 12), (3, 6)]      // Vertex 2 edges
};

Matching result = FastHungarian.MaximumWeightMatching(
    leftCount: 3, 
    rightCount: 4, 
    adjacencyList
);

Console.WriteLine($"Maximum weight: {result.WeightSum}");
// result.LeftPairs[i] = matched right vertex for left vertex i
// result.RightPairs[j] = matched left vertex for right vertex j
```

### Handling Non-Square Matrices

```csharp
// More workers than tasks: some workers won't be assigned
int[,] costs = new int[5, 3];  // 5 workers, 3 tasks
int[] assignments = FastHungarian.FindAssignments(costs);
// assignments.Length == 5
// assignments[i] == -1 if worker i is not assigned

// More tasks than workers: all workers assigned, some tasks unassigned
int[,] costs2 = new int[3, 5];  // 3 workers, 5 tasks
int[] assignments2 = FastHungarian.FindAssignments(costs2);
// assignments2.Length == 3
// All assignments2[i] >= 0
```

## Algorithm Details

FastHungarian implements modified **Kwok's 2025 algorithm** for maximum weight matching with several optimizations:

1. **Flat adjacency list** - Better cache locality vs nested lists
2. **Edge retention** (Theorem 4.2.1) - Reduces edges to O(|L|) per vertex using QuickSelect
3. **Pre-computed labels** - Eliminates redundant scans
4. **Incremental slack tracking** - Avoids repeated minimum calculations
5. **Ring buffer BFS** - Reduces queue overhead
6. **Span<T> bounds check elimination** (.NET 8+) - Zero-overhead array access
7. **Branchless operations** - Better CPU pipelining

### Complexity

- **Time**: O(|L| × |R| × min(|L|, |R|)) worst case, typically much faster in practice
- **Space**: O(|L| × min(|L|, |R|)) due to edge retention
- **Classical Hungarian**: O(|L|³) or O(|L|² × |R|)

## Performance

Benchmark results on Intel Core i7-8700 (Coffee Lake), baseline (referred to as "vanilla") implementation is available [here](https://github.com/vivet/HungarianAlgorithm):

<img alt="Benchmark" src="https://github.com/user-attachments/assets/d2b2c004-ef64-49de-bff8-dc5af56a1b28" />

| Method                          | Mean          | Error       | StdDev      | Ratio | RatioSD | Gen0    | Gen1    | Gen2    | Allocated | Alloc Ratio |
|-------------------------------- |--------------:|------------:|------------:|------:|--------:|--------:|--------:|--------:|----------:|------------:|
| FastHungarian_Large_100x100     |    401.923 us |   5.5506 us |   5.1920 us |  0.06 |    0.00 | 19.5313 |  6.3477 |       - | 122.93 KB |        0.96 |
| Vanilla_Large_100x100           |  6,447.195 us |  86.5501 us |  72.2732 us |  1.00 |    0.02 | 15.6250 |       - |       - | 127.72 KB |        1.00 |
|                                 |               |             |             |       |         |         |         |         |           |             |
| FastHungarian_Large_150x150     |    786.428 us |   7.9380 us |   6.6286 us |  0.03 |    0.00 | 84.9609 | 84.9609 | 84.9609 | 272.08 KB |        0.95 |
| Vanilla_Large_150x150           | 27,677.698 us | 456.3057 us | 404.5031 us |  1.00 |    0.02 | 62.5000 | 62.5000 | 62.5000 | 286.72 KB |        1.00 |
|                                 |               |             |             |       |         |         |         |         |           |             |
| FastHungarian_Medium_50x50      |     91.058 us |   1.7370 us |   1.8585 us |  0.13 |    0.00 |  5.2490 |  0.3662 |       - |  32.41 KB |        1.01 |
| Vanilla_Medium_50x50            |    680.347 us |  12.9339 us |  11.4655 us |  1.00 |    0.02 |  4.8828 |       - |       - |  32.22 KB |        1.00 |
|                                 |               |             |             |       |         |         |         |         |           |             |
| FastHungarian_Rect_100x50       |    109.635 us |   0.8397 us |   0.7012 us |  0.10 |    0.00 | 10.3760 |  0.9766 |       - |  63.81 KB |        0.76 |
| Vanilla_Rect_100x50             |  1,084.285 us |  13.4670 us |  12.5970 us |  1.00 |    0.02 | 11.7188 |       - |       - |  83.98 KB |        1.00 |
|                                 |               |             |             |       |         |         |         |         |           |             |
| FastHungarian_Rect_50x100       |     84.625 us |   1.5834 us |   1.4811 us |  0.16 |    0.01 |  7.0801 |  0.9766 |       - |  43.83 KB |        0.68 |
| Vanilla_Rect_50x100             |    522.524 us |  10.2609 us |  17.1437 us |  1.00 |    0.05 |  9.7656 |       - |       - |     64 KB |        1.00 |
|                                 |               |             |             |       |         |         |         |         |           |             |
| FastHungarian_Small_10x10       |      1.824 us |   0.0363 us |   0.0766 us |  0.39 |    0.02 |  0.3529 |       - |       - |   2.17 KB |        1.43 |
| Vanilla_Small_10x10             |      4.733 us |   0.0572 us |   0.0507 us |  1.00 |    0.01 |  0.2441 |       - |       - |   1.52 KB |        1.00 |
|                                 |               |             |             |       |         |         |         |         |           |             |
| FastHungarian_Sparse_100x100    |     89.095 us |   1.7469 us |   2.6677 us |  0.01 |    0.00 | 19.8975 |  6.5918 |       - | 122.93 KB |        0.96 |
| Vanilla_Sparse_100x100          |  7,015.159 us |  70.4273 us |  62.4320 us |  1.00 |    0.01 | 15.6250 |       - |       - | 127.72 KB |        1.00 |
|                                 |               |             |             |       |         |         |         |         |           |             |
| FastHungarian_WideRange_100x100 |    362.584 us |   6.7749 us |   9.7164 us |  0.04 |    0.00 | 19.5313 |  6.3477 |       - | 122.93 KB |        0.96 |
| Vanilla_WideRange_100x100       |  8,564.100 us | 167.5207 us | 302.0744 us |  1.00 |    0.05 | 15.6250 |       - |       - | 127.72 KB |        1.00 |

Full benchmark results and reproduction available in the [benchmark project](https://github.com/lofcz/FastHungarian/tree/main/src/FastHungarian/FastHungarian.Benchmark).

## Testing

FastHungarian is thoroughly tested with **11 comprehensive fuzz test suites** covering:

- ✅ Square matrices (2×2 to 200×200)
- ✅ Non-square matrices (all aspect ratios)
- ✅ Extreme shapes (1×N, N×1, 10:1 ratios)
- ✅ Edge case patterns (diagonal, sparse, bimodal, outliers)
- ✅ Degenerate cases (1×1, single row/column, near-overflow)
- ✅ **Total: 350+ test iterations, thousands of matrix configurations**

All tests verify correctness against the proven Hungarian algorithm implementation from the `HungarianAlgorithm` NuGet package.

## Limitations

- **Integer weights**: Currently supports `int` weights. For floating-point weights, scale to integers.

The algorithm handles:
- ✅ Non-square matrices (automatic transposition)
- ✅ Unbalanced assignments (more workers than tasks or vice versa)
- ✅ Zero costs and identical values
- ✅ Large cost ranges (tested up to `int.MaxValue / 2`)

## Contributing

If you are looking to add new functionality, please open an issue first to verify your intent is aligned with the scope of the project. The library is covered by **comprehensive fuzz tests** with over 350 test iterations. Please run them against your work before proposing changes:

```powershell
dotnet test
```

When reporting issues, providing a minimal reproduction with the specific cost matrix that fails greatly reduces turnaround time.

## References

- **Kwok, J. T. (2025)**. [An Improved Algorithm for Maximum Weight Matching in Bipartite Graphs](https://www.arxiv.org/pdf/2502.20889)
- Based on the classical Hungarian algorithm (Kuhn-Munkres algorithm)

## License

This library is licensed under the [MIT](https://github.com/lofcz/FastHungarian/blob/main/LICENSE) license. 💜


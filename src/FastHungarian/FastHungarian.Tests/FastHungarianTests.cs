using HungarianAlgorithm;

namespace FastHungarian.Tests;

[TestFixture]
public class FastHungarianTests
{
    private static void AssertImplementationsMatch(int[,] costs, string testName)
    {
        int[,] costsCurrent = (int[,])costs.Clone();
        int[,] costsVanilla = (int[,])costs.Clone();
        
        int[] resultCurrent = FastHungarian.FindAssignments(costsCurrent);
        int[]? resultVanilla = costsVanilla.FindAssignments();
        
        int currentCost = 0;
        int vanillaCost = 0;
        
        for (int i = 0; i < resultCurrent.Length; i++)
        {
            if (resultCurrent[i] != -1) currentCost += costs[i, resultCurrent[i]];
            if (resultVanilla[i] != -1) vanillaCost += costs[i, resultVanilla[i]];
        }
        
        Assert.That(currentCost, Is.EqualTo(vanillaCost), 
            $"{testName}: Current implementation should match vanilla");
    }
    
    private static (int current, int vanilla) GetCostsFromAllImplementations(int[,] costs)
    {
        int[,] costsCurrent = (int[,])costs.Clone();
        int[,] costsVanilla = (int[,])costs.Clone();
        
        int[] resultCurrent = FastHungarian.FindAssignments(costsCurrent);
        int[]? resultVanilla = costsVanilla.FindAssignments();
        
        int currentCost = 0;
        int vanillaCost = 0;
        
        for (int i = 0; i < resultCurrent.Length; i++)
        {
            if (resultCurrent[i] != -1) currentCost += costs[i, resultCurrent[i]];
            if (resultVanilla[i] != -1) vanillaCost += costs[i, resultVanilla[i]];
        }
        
        return (currentCost, vanillaCost);
    }

    [Test]
    public void BasicSquareMatrix_3x3()
    {
        int[,] costs = new[,]
        {
            { 1, 2, 3 },
            { 2, 4, 6 },
            { 3, 6, 9 }
        };

        AssertImplementationsMatch(costs, "BasicSquareMatrix_3x3");
    }

    [Test]
    public void BasicSquareMatrix_4x4()
    {
        int[,] costs = new[,]
        {
            { 10, 25, 15, 20 },
            { 15, 30, 5, 15 },
            { 35, 20, 12, 24 },
            { 17, 25, 24, 20 }
        };

        AssertImplementationsMatch(costs, "BasicSquareMatrix_4x4");
    }

    [Test]
    public void NonSquareMatrix_MoreRows()
    {
        int[,] costs = new[,]
        {
            { 5, 10, 15 },
            { 10, 15, 20 },
            { 15, 20, 25 },
            { 20, 25, 30 }
        };

        int[] result = FastHungarian.FindAssignments(costs);
        costs.FindAssignments();

        Assert.That(result, Has.Length.EqualTo(4));
        
        int matchedCount = 0;
        for (int i = 0; i < 4; i++)
        {
            if (result[i] != -1) matchedCount++;
        }
        
        Assert.That(matchedCount, Is.EqualTo(3));
    }

    [Test]
    public void NonSquareMatrix_MoreColumns()
    {
        int[,] costs = new[,]
        {
            { 5, 10, 1, 15, 20 },
            { 10, 2, 20, 25, 30 },
            { 15, 20, 25, 3, 35 }
        };

        int[] result = FastHungarian.FindAssignments(costs);
        int[]? vanilla = costs.FindAssignments();

        Assert.That(result, Has.Length.EqualTo(3));
        
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result[0], Is.GreaterThanOrEqualTo(0));
            Assert.That(result[1], Is.GreaterThanOrEqualTo(0));
            Assert.That(result[2], Is.GreaterThanOrEqualTo(0));
        }

        int kwokCost = 0;
        int vanillaCost = 0;
        for (int i = 0; i < 3; i++)
        {
            if (result[i] != -1) kwokCost += costs[i, result[i]];
            if (vanilla[i] != -1) vanillaCost += costs[i, vanilla[i]];
        }
        
        Assert.That(kwokCost, Is.EqualTo(vanillaCost));
    }

    [Test]
    public void SingleElement()
    {
        int[,] costs = new[,] { { 42 } };
        int[] result = FastHungarian.FindAssignments(costs);
        
        Assert.That(result, Has.Length.EqualTo(1));
        Assert.That(result[0], Is.Zero);
    }

    [Test]
    public void ZeroCosts()
    {
        int[,] costs = new[,]
        {
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 }
        };

        int[] result = FastHungarian.FindAssignments(costs);
        
        Assert.That(result, Has.Length.EqualTo(3));
        
        bool[] used = new bool[3];
        for (int i = 0; i < 3; i++)
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result[i], Is.GreaterThanOrEqualTo(0).And.LessThan(3));
                Assert.That(used[result[i]], Is.False, "Each column should be used once");
            }
            used[result[i]] = true;
        }
    }

    [Test]
    public void LargeCosts()
    {
        int[,] costs = new[,]
        {
            { 1000, 2000, 3000 },
            { 4000, 5000, 6000 },
            { 7000, 8000, 9000 }
        };

        int[] result = FastHungarian.FindAssignments(costs);
        int[]? vanilla = costs.FindAssignments();

        int kwokCost = costs[0, result[0]] + costs[1, result[1]] + costs[2, result[2]];
        int vanillaCost = costs[0, vanilla[0]] + costs[1, vanilla[1]] + costs[2, vanilla[2]];
        
        Assert.That(kwokCost, Is.EqualTo(vanillaCost));
    }

    [Test]
    public void IdenticalRows()
    {
        int[,] costs = new[,]
        {
            { 10, 20, 30 },
            { 10, 20, 30 },
            { 10, 20, 30 }
        };

        int[] result = FastHungarian.FindAssignments(costs);
        
        Assert.That(result, Has.Length.EqualTo(3));
        
        bool[] used = new bool[3];
        for (int i = 0; i < 3; i++)
        {
            used[result[i]] = true;
        }
        Assert.That(used, Is.All.True, "All columns should be used");
    }

    [Test]
    public void AdjacencyListAPI_Sparse()
    {
        List<List<(int vertex, int weight)>> adj =
        [
            [(0, 10), (1, 5)],
            [(1, 8), (2, 7)],
            [(2, 12), (3, 3)]
        ];

        Matching result = FastHungarian.MaximumWeightMatching(3, 4, adj);

        Assert.That(result.LeftPairs, Has.Length.EqualTo(3));
        Assert.That(result.RightPairs, Has.Length.EqualTo(4));
        
        Assert.That(result.LeftPairs[0], Is.GreaterThanOrEqualTo(0));
        Assert.That(result.LeftPairs[1], Is.GreaterThanOrEqualTo(0));
        Assert.That(result.LeftPairs[2], Is.GreaterThanOrEqualTo(0));
        
        Assert.That(result.WeightSum, Is.GreaterThan(0));
    }

    [Test]
    public void AdjacencyListAPI_Complete()
    {
        List<List<(int vertex, int weight)>> adj =
        [
            [(0, 5), (1, 10), (2, 15)],
            [(0, 8), (1, 12), (2, 16)]
        ];

        Matching result = FastHungarian.MaximumWeightMatching(2, 3, adj);

        Assert.That(result.LeftPairs, Has.Length.EqualTo(2));
        
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.LeftPairs[0], Is.GreaterThanOrEqualTo(0));
            Assert.That(result.LeftPairs[1], Is.GreaterThanOrEqualTo(0));
        }
        
        Assert.That(result.LeftPairs[0], Is.Not.EqualTo(result.LeftPairs[1]));
    }

    [Test]
    public void FindAssignmentsWithWeight_ReturnsCorrectCost()
    {
        int[,] costs = new[,]
        {
            { 10, 25, 15 },
            { 15, 30, 5 },
            { 35, 20, 12 }
        };

        Matching result = FastHungarian.FindAssignmentsWithWeight(costs);
        
        Assert.That(result.LeftPairs, Has.Length.EqualTo(3));
        
        int calculatedCost = 0;
        for (int i = 0; i < 3; i++)
        {
            if (result.LeftPairs[i] != -1)
            {
                calculatedCost += costs[i, result.LeftPairs[i]];
            }
        }
        
        Assert.That(result.WeightSum, Is.EqualTo(calculatedCost));
    }

    [Test]
    public void CompareWithVanilla_RandomMatrix()
    {
        Random random = new Random(42);
        int[,] costs = new int[5, 5];
        
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                costs[i, j] = random.Next(1, 100);
            }
        }

        int[] result = FastHungarian.FindAssignments(costs);
        int[]? vanilla = costs.FindAssignments();

        int kwokCost = 0;
        int vanillaCost = 0;
        for (int i = 0; i < 5; i++)
        {
            if (result[i] != -1) kwokCost += costs[i, result[i]];
            if (vanilla[i] != -1) vanillaCost += costs[i, vanilla[i]];
        }
        
        Assert.That(kwokCost, Is.EqualTo(vanillaCost), 
            "Both algorithms should find the same optimal cost");
    }

    [Test]
    public void EdgeRetentionOptimization_ManyEdges()
    {
        List<List<(int vertex, int weight)>> adj =
        [
            [(0, 1), (1, 2), (2, 3), (3, 4), (4, 5)],
            [(0, 10)],
            [(1, 10)]
        ];

        Matching result = FastHungarian.MaximumWeightMatching(3, 5, adj);
        
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.LeftPairs[0], Is.GreaterThanOrEqualTo(0));
            Assert.That(result.LeftPairs[1], Is.GreaterThanOrEqualTo(0));
            Assert.That(result.LeftPairs[2], Is.GreaterThanOrEqualTo(0));
        }
    }

    [Test]
    public void EmptyEdges_ShouldHandleGracefully()
    {
        List<List<(int vertex, int weight)>> adj =
        [
            [],
            [(0, 5)],
            [(1, 10)]
        ];

        Matching result = FastHungarian.MaximumWeightMatching(3, 2, adj);
        Assert.That(result.LeftPairs[0], Is.EqualTo(-1));
    }

    [Test]
    public void NegativeWeightsIgnored_InAdjacencyList()
    {
        List<List<(int vertex, int weight)>> adj =
        [
            [(0, 10), (1, -5)],
            [(0, -3), (1, 8)]
        ];

        Matching result = FastHungarian.MaximumWeightMatching(2, 2, adj);
        Assert.That(result.WeightSum, Is.GreaterThan(0));
    }

    [Test]
    public void VeryLargeSparseGraph()
    {
        List<List<(int vertex, int weight)>> adj = [];
        Random random = new Random(123);
        
        for (int i = 0; i < 10; i++)
        {
            List<(int vertex, int weight)> edges = [];
      
            for (int j = 0; j < 3; j++)
            {
                int rightVertex = random.Next(20);
                int weight = random.Next(1, 100);
                edges.Add((rightVertex, weight));
            }
            adj.Add(edges);
        }

        Matching result = FastHungarian.MaximumWeightMatching(10, 20, adj);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.LeftPairs, Has.Length.EqualTo(10));
            Assert.That(result.RightPairs, Has.Length.EqualTo(20));
        }
    }

    [Test]
    public void CompareWithVanilla_LargeMatrix_10x10()
    {
        Random random = new Random(999);
        int[,] costs = new int[10, 10];
        
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                costs[i, j] = random.Next(1, 1000);
            }
        }

        int[] result = FastHungarian.FindAssignments(costs);
        int[]? vanilla = costs.FindAssignments();

        int kwokCost = 0;
        int vanillaCost = 0;
        for (int i = 0; i < 10; i++)
        {
            if (result[i] != -1) kwokCost += costs[i, result[i]];
            if (vanilla[i] != -1) vanillaCost += costs[i, vanilla[i]];
        }
        
        Assert.That(kwokCost, Is.EqualTo(vanillaCost), 
            "Both algorithms should find the same optimal cost on larger matrices");
    }

    [Test]
    public void FuzzTest_SquareMatrices_100Iterations()
    {
        Random random = new Random(12345);
        
        for (int iteration = 0; iteration < 100; iteration++)
        {
            int size = random.Next(2, 15);
            int[,] costs = new int[size, size];
            
            int maxValue = random.Next(10, 1000);
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    costs[i, j] = random.Next(1, maxValue);
                }
            }
            
            (int currentCost, int vanillaCost) = GetCostsFromAllImplementations(costs);
            
            Assert.That(currentCost, Is.EqualTo(vanillaCost), 
                $"Iteration {iteration}: Current vs Vanilla, Size {size}x{size}, MaxValue {maxValue}");
        }
    }

    [Test]
    public void FuzzTest_NonSquareMatrices_100Iterations()
    {
        Random random = new Random(54321);
        
        for (int iteration = 0; iteration < 100; iteration++)
        {
            int rows = random.Next(2, 15);
            int cols = random.Next(2, 15);
            int[,] costs = new int[rows, cols];
            
            int maxValue = random.Next(10, 1000);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    costs[i, j] = random.Next(1, maxValue);
                }
            }
            
            int[,] costsCopy = (int[,])costs.Clone();
            
            int[] result = FastHungarian.FindAssignments(costs);
            int[]? vanilla = costsCopy.FindAssignments();
            
            int kwokCost = 0;
            int vanillaCost = 0;
            for (int i = 0; i < rows; i++)
            {
                if (result[i] != -1) kwokCost += costs[i, result[i]];
                if (vanilla[i] != -1) vanillaCost += costs[i, vanilla[i]];
            }
            
            Assert.That(kwokCost, Is.EqualTo(vanillaCost), 
                $"Iteration {iteration}: Size {rows}x{cols}, MaxValue {maxValue}");
        }
    }

    [Test]
    public void FuzzTest_EdgeCases_50Iterations()
    {
        Random random = new Random(99999);
        
        for (int iteration = 0; iteration < 50; iteration++)
        {
            int rows = random.Next(1, 20);
            int cols = random.Next(1, 20);
            int[,] costs = new int[rows, cols];
            
            // Test various edge cases
            int scenario = iteration % 5;
            switch (scenario)
            {
                case 0: // All same values
                    int value = random.Next(1, 100);
                    for (int i = 0; i < rows; i++)
                        for (int j = 0; j < cols; j++)
                            costs[i, j] = value;
                    break;
                    
                case 1: // Very small values
                    for (int i = 0; i < rows; i++)
                        for (int j = 0; j < cols; j++)
                            costs[i, j] = random.Next(1, 5);
                    break;
                    
                case 2: // Very large values
                    for (int i = 0; i < rows; i++)
                        for (int j = 0; j < cols; j++)
                            costs[i, j] = random.Next(10000, 100000);
                    break;
                    
                case 3: // Sparse (many same values with few outliers)
                    int baseVal = random.Next(1, 10);
                    for (int i = 0; i < rows; i++)
                        for (int j = 0; j < cols; j++)
                            costs[i, j] = random.Next(100) < 10 ? random.Next(1, 100) : baseVal;
                    break;
                    
                case 4: // Wide range
                    for (int i = 0; i < rows; i++)
                        for (int j = 0; j < cols; j++)
                            costs[i, j] = random.Next(1, 100000);
                    break;
            }
            
            int[,] costsCopy = (int[,])costs.Clone();
            
            int[] result = FastHungarian.FindAssignments(costs);
            int[]? vanilla = costsCopy.FindAssignments();
            
            int kwokCost = 0;
            int vanillaCost = 0;
            for (int i = 0; i < rows; i++)
            {
                if (result[i] != -1) kwokCost += costs[i, result[i]];
                if (vanilla[i] != -1) vanillaCost += costs[i, vanilla[i]];
            }
            
            Assert.That(kwokCost, Is.EqualTo(vanillaCost), 
                $"Iteration {iteration}: Scenario {scenario}, Size {rows}x{cols}");
        }
    }

    [Test]
    public void FuzzTest_MediumMatrices_50Iterations()
    {
        Random random = new Random(77777);
        
        for (int iteration = 0; iteration < 50; iteration++)
        {
            int rows = random.Next(20, 80);
            int cols = random.Next(20, 80);
            int[,] costs = new int[rows, cols];
            
            int maxValue = random.Next(100, 5000);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    costs[i, j] = random.Next(1, maxValue);
                }
            }
            
            int[,] costsCopy = (int[,])costs.Clone();
            
            int[] result = FastHungarian.FindAssignments(costs);
            int[]? vanilla = costsCopy.FindAssignments();
            
            int kwokCost = 0;
            int vanillaCost = 0;
            for (int i = 0; i < rows; i++)
            {
                if (result[i] != -1) kwokCost += costs[i, result[i]];
                if (vanilla[i] != -1) vanillaCost += costs[i, vanilla[i]];
            }
            
            Assert.That(kwokCost, Is.EqualTo(vanillaCost), 
                $"Iteration {iteration}: Size {rows}x{cols}, MaxValue {maxValue}");
        }
    }

    [Test]
    public void FuzzTest_LargeMatrices_30Iterations()
    {
        Random random = new Random(88888);
        
        for (int iteration = 0; iteration < 30; iteration++)
        {
            int rows = random.Next(80, 150);
            int cols = random.Next(80, 150);
            int[,] costs = new int[rows, cols];
            
            int maxValue = random.Next(100, 10000);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    costs[i, j] = random.Next(1, maxValue);
                }
            }
            
            int[,] costsCopy = (int[,])costs.Clone();
            
            int[] result = FastHungarian.FindAssignments(costs);
            int[]? vanilla = costsCopy.FindAssignments();
            
            int kwokCost = 0;
            int vanillaCost = 0;
            for (int i = 0; i < rows; i++)
            {
                if (result[i] != -1) kwokCost += costs[i, result[i]];
                if (vanilla[i] != -1) vanillaCost += costs[i, vanilla[i]];
            }
            
            Assert.That(kwokCost, Is.EqualTo(vanillaCost), 
                $"Iteration {iteration}: Size {rows}x{cols}, MaxValue {maxValue}");
        }
    }

    [Test]
    public void FuzzTest_VeryLargeMatrices_20Iterations()
    {
        Random random = new Random(11111);
        
        for (int iteration = 0; iteration < 20; iteration++)
        {
            int rows = random.Next(100, 200);
            int cols = random.Next(100, 200);
            int[,] costs = new int[rows, cols];
            
            int maxValue = random.Next(100, 10000);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    costs[i, j] = random.Next(1, maxValue);
                }
            }
            
            int[,] costsCopy = (int[,])costs.Clone();
            
            int[] result = FastHungarian.FindAssignments(costs);
            int[]? vanilla = costsCopy.FindAssignments();
            
            int kwokCost = 0;
            int vanillaCost = 0;
            for (int i = 0; i < rows; i++)
            {
                if (result[i] != -1) kwokCost += costs[i, result[i]];
                if (vanilla[i] != -1) vanillaCost += costs[i, vanilla[i]];
            }
            
            Assert.That(kwokCost, Is.EqualTo(vanillaCost), 
                $"Iteration {iteration}: Size {rows}x{cols}, MaxValue {maxValue}");
        }
    }

    [Test]
    public void FuzzTest_RectangularMatrices_Large_30Iterations()
    {
        Random random = new Random(22222);
        
        for (int iteration = 0; iteration < 30; iteration++)
        {
            // Test highly rectangular matrices
            bool tallMatrix = random.Next(2) == 0;
            int rows, cols;
            
            if (tallMatrix)
            {
                rows = random.Next(100, 200);
                cols = random.Next(10, 50);
            }
            else
            {
                rows = random.Next(10, 50);
                cols = random.Next(100, 200);
            }
            
            int[,] costs = new int[rows, cols];
            
            int maxValue = random.Next(100, 10000);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    costs[i, j] = random.Next(1, maxValue);
                }
            }
            
            int[,] costsCopy = (int[,])costs.Clone();
            
            int[] result = FastHungarian.FindAssignments(costs);
            int[]? vanilla = costsCopy.FindAssignments();
            
            int kwokCost = 0;
            int vanillaCost = 0;
            for (int i = 0; i < rows; i++)
            {
                if (result[i] != -1) kwokCost += costs[i, result[i]];
                if (vanilla[i] != -1) vanillaCost += costs[i, vanilla[i]];
            }
            
            Assert.That(kwokCost, Is.EqualTo(vanillaCost), 
                $"Iteration {iteration}: Size {rows}x{cols}, MaxValue {maxValue}");
        }
    }

    [Test]
    public void FuzzTest_ExtremeShapes_50Iterations()
    {
        Random random = new Random(33333);
        
        for (int iteration = 0; iteration < 50; iteration++)
        {
            int rows, cols;
            
            // Test extreme aspect ratios
            int shapeType = random.Next(6);
            switch (shapeType)
            {
                case 0: // Very tall (Nx1)
                    rows = random.Next(10, 100);
                    cols = 1;
                    break;
                case 1: // Very wide (1xN)
                    rows = 1;
                    cols = random.Next(10, 100);
                    break;
                case 2: // Tall (10:1 ratio)
                    cols = random.Next(5, 20);
                    rows = cols * random.Next(8, 12);
                    break;
                case 3: // Wide (1:10 ratio)
                    rows = random.Next(5, 20);
                    cols = rows * random.Next(8, 12);
                    break;
                case 4: // Very small but rectangular
                    rows = random.Next(2, 5);
                    cols = random.Next(2, 5);
                    break;
                default: // Extreme large rectangular
                    rows = random.Next(150, 250);
                    cols = random.Next(10, 30);
                    if (random.Next(2) == 0)
                        (rows, cols) = (cols, rows); // Swap
                    break;
            }
            
            int[,] costs = new int[rows, cols];
            int maxValue = random.Next(10, 1000);
            
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    costs[i, j] = random.Next(1, maxValue);
                }
            }
            
            (int currentCost, int vanillaCost) = GetCostsFromAllImplementations(costs);
            
            Assert.That(currentCost, Is.EqualTo(vanillaCost), 
                $"Iteration {iteration}: Current vs Vanilla, Shape {rows}x{cols}, Type {shapeType}");
        }
    }

    [Test]
    public void FuzzTest_EdgeCasePatterns_50Iterations()
    {
        Random random = new Random(44444);
        
        for (int iteration = 0; iteration < 50; iteration++)
        {
            int rows = random.Next(10, 100);
            int cols = random.Next(10, 100);
            int[,] costs = new int[rows, cols];
            
            int patternType = random.Next(10);
            
            switch (patternType)
            {
                case 0:
                    break;
                    
                case 1:
                    int sameValue = random.Next(1, 100);
                    for (int i = 0; i < rows; i++)
                        for (int j = 0; j < cols; j++)
                            costs[i, j] = sameValue;
                    break;
                    
                case 2:
                    for (int i = 0; i < rows; i++)
                    {
                        for (int j = 0; j < cols; j++)
                        {
                            costs[i, j] = i == j ? 1 : random.Next(10, 100);
                        }
                    }
                    break;
                    
                case 3:
                    for (int i = 0; i < rows; i++)
                    {
                        for (int j = 0; j < cols; j++)
                        {
                            costs[i, j] = i + j == Math.Min(rows, cols) - 1 ? 1 : random.Next(10, 100);
                        }
                    }
                    break;
                    
                case 4:
                    for (int i = 0; i < rows; i++)
                    {
                        for (int j = 0; j < cols; j++)
                        {
                            costs[i, j] = random.NextDouble() < 0.05 ? random.Next(1, 10) : random.Next(1000, 10000);
                        }
                    }
                    break;
                    
                case 5:
                    for (int i = 0; i < rows; i++)
                    {
                        for (int j = 0; j < cols; j++)
                        {
                            costs[i, j] = random.Next(2) == 0 ? random.Next(1, 10) : random.Next(990, 1000);
                        }
                    }
                    break;
                    
                case 6:
                    for (int i = 0; i < rows; i++)
                    {
                        for (int j = 0; j < cols; j++)
                        {
                            costs[i, j] = random.Next(10, 100);
                        }
                    }
                    int cheapRow = random.Next(rows);
                    for (int j = 0; j < cols; j++)
                        costs[cheapRow, j] = random.Next(0, 2);
                    break;
                    
                case 7:
                    for (int i = 0; i < rows; i++)
                    {
                        for (int j = 0; j < cols; j++)
                        {
                            costs[i, j] = i * 10 + j;
                        }
                    }
                    break;
                    
                case 8:
                    int midRow = rows / 2;
                    int midCol = cols / 2;
                    for (int i = 0; i < rows; i++)
                    {
                        for (int j = 0; j < cols; j++)
                        {
                            if (i < midRow && j < midCol || i >= midRow && j >= midCol)
                                costs[i, j] = random.Next(1, 10);
                            else
                                costs[i, j] = random.Next(50, 100);
                        }
                    }
                    break;
                    
                default:
                    for (int i = 0; i < rows; i++)
                    {
                        for (int j = 0; j < cols; j++)
                        {
                            if (random.NextDouble() < 0.02)
                                costs[i, j] = int.MaxValue / 2;
                            else
                                costs[i, j] = random.Next(1, 100);
                        }
                    }
                    break;
            }
            
            (int currentCost, int vanillaCost) = GetCostsFromAllImplementations(costs);
            
            Assert.That(currentCost, Is.EqualTo(vanillaCost), 
                $"Iteration {iteration}: Current vs Vanilla, Size {rows}x{cols}, Pattern {patternType}");
        }
    }

    [Test]
    public void FuzzTest_RandomShapesAndValues_100Iterations()
    {
        Random random = new Random(55555);
        
        for (int iteration = 0; iteration < 100; iteration++)
        {
            int rows = random.Next(1, 150);
            int cols = random.Next(1, 150);
            int[,] costs = new int[rows, cols];
            
            int distributionType = random.Next(5);
            
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    costs[i, j] = distributionType switch
                    {
                        0 => random.Next(0, 100),
                        1 => random.Next(0, 100000),
                        2 => (int)Math.Pow(random.Next(1, 100), 2),
                        3 => random.Next(1, 11) * random.Next(1, 11) * random.Next(1, 11),
                        _ => random.Next(0, random.Next(10, 10000))
                    };
                }
            }
            
            (int currentCost, int vanillaCost) = GetCostsFromAllImplementations(costs);
            
            Assert.That(currentCost, Is.EqualTo(vanillaCost), 
                $"Iteration {iteration}: Current vs Vanilla, Size {rows}x{cols}, Distribution {distributionType}");
        }
    }

    [Test]
    public void FuzzTest_DegenerateCases_30Iterations()
    {
        Random random = new Random(66666);
        
        for (int iteration = 0; iteration < 30; iteration++)
        {
            int testCase = random.Next(8);
            int[,] costs;
            
            switch (testCase)
            {
                case 0:
                    costs = new int[1, 1];
                    costs[0, 0] = random.Next(0, 100);
                    break;
                case 1:
                    costs = new int[2, 2];
                    costs[0, 0] = 1;
                    costs[0, 1] = 100;
                    costs[1, 0] = 100;
                    costs[1, 1] = 1;
                    break;
                case 2:
                    int cols2 = random.Next(5, 50);
                    costs = new int[1, cols2];
                    for (int j = 0; j < cols2; j++)
                        costs[0, j] = random.Next(1, 100);
                    break;
                case 3:
                    int rows3 = random.Next(5, 50);
                    costs = new int[rows3, 1];
                    for (int i = 0; i < rows3; i++)
                        costs[i, 0] = random.Next(1, 100);
                    break;
                case 4:
                    int size4 = random.Next(5, 20);
                    costs = new int[size4, size4];
                    for (int i = 0; i < size4; i++)
                        for (int j = 0; j < size4; j++)
                            costs[i, j] = int.MaxValue / 2 - random.Next(0, 1000);
                    break;
                case 5:
                    int size5 = random.Next(5, 50);
                    costs = new int[size5, size5];
                    for (int i = 0; i < size5; i++)
                    {
                        for (int j = 0; j < size5; j++)
                        {
                            costs[i, j] = i == j ? 0 : 1000;
                        }
                    }
                    break;
                case 6:
                    int size6 = random.Next(5, 50);
                    costs = new int[size6, size6];
                    for (int i = 0; i < size6; i++)
                    {
                        for (int j = 0; j < size6; j++)
                        {
                            costs[i, j] = i + j == size6 - 1 ? 0 : 1000;
                        }
                    }
                    break;
                default:
                    int rows7 = random.Next(10, 30);
                    int cols7 = random.Next(10, 30);
                    costs = new int[rows7, cols7];
                    for (int i = 0; i < rows7; i++)
                    {
                        for (int j = 0; j < cols7; j++)
                        {
                            costs[i, j] = 1000;
                        }
                    }
                    int perfectI = random.Next(rows7);
                    int perfectJ = random.Next(cols7);
                    costs[perfectI, perfectJ] = 0;
                    break;
            }
            
            (int currentCost,int vanillaCost) = GetCostsFromAllImplementations(costs);
            
            Assert.That(currentCost, Is.EqualTo(vanillaCost), 
                $"Iteration {iteration}: Current vs Vanilla, Degenerate case {testCase}");
        }
    }
}


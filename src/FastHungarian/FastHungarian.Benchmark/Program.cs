using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using HungarianAlgorithm;

namespace FastHungarian.Benchmark;

class Program
{
    static void Main(string[] args)
    {
        RunFullBenchmark();
    }
    
    public static void RunFullBenchmark()
    {
        BenchmarkRunner.Run<HungarianBenchmarks>();
    }
}

[MemoryDiagnoser]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
public class HungarianBenchmarks
{
    private int[,] small10X10 = null!;
    private int[,] medium50X50 = null!;
    private int[,] large100X100 = null!;
    private int[,] large150X150 = null!;
    private int[,] rect50X100 = null!;
    private int[,] rect100X50 = null!;
    private int[,] sparse100X100 = null!;
    private int[,] wideRange100X100 = null!;

    [GlobalSetup]
    public void Setup()
    {
        Random random = new Random(42);
        
        small10X10 = GenerateRandomMatrix(10, 10, 1, 100, random);
        medium50X50 = GenerateRandomMatrix(50, 50, 1, 500, random);

        large100X100 = GenerateRandomMatrix(100, 100, 1, 1000, random);
        large150X150 = GenerateRandomMatrix(150, 150, 1, 1500, random);
        
        rect50X100 = GenerateRandomMatrix(50, 100, 1, 500, random);
        rect100X50 = GenerateRandomMatrix(100, 50, 1, 500, random);
        
        sparse100X100 = GenerateSparseMatrix(100, 100, random);

        wideRange100X100 = GenerateRandomMatrix(100, 100, 1, 100000, random);
    }

    private static int[,] GenerateRandomMatrix(int rows, int cols, int minValue, int maxValue, Random random)
    {
        int[,] matrix = new int[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                matrix[i, j] = random.Next(minValue, maxValue);
            }
        }
        return matrix;
    }

    private static int[,] GenerateSparseMatrix(int rows, int cols, Random random)
    {
        int[,] matrix = new int[rows, cols];
        const int baseValue = 10;
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                matrix[i, j] = random.Next(100) < 90 ? baseValue : random.Next(1, 100);
            }
        }
        return matrix;
    }
    
    [Benchmark]
    [BenchmarkCategory("Small_10x10")]
    public int[] FastHungarian_Small_10x10()
    {
        int[,] copy = (int[,])small10X10.Clone();
        return FastHungarian.FindAssignments(copy);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Small_10x10")]
    public int[] Vanilla_Small_10x10()
    {
        int[,] copy = (int[,])small10X10.Clone();
        return copy.FindAssignments();
    }
    
    [Benchmark]
    [BenchmarkCategory("Medium_50x50")]
    public int[] FastHungarian_Medium_50x50()
    {
        int[,] copy = (int[,])medium50X50.Clone();
        return FastHungarian.FindAssignments(copy);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Medium_50x50")]
    public int[] Vanilla_Medium_50x50()
    {
        int[,] copy = (int[,])medium50X50.Clone();
        return copy.FindAssignments();
    }
    
    [Benchmark]
    [BenchmarkCategory("Large_100x100")]
    public int[] FastHungarian_Large_100x100()
    {
        int[,] copy = (int[,])large100X100.Clone();
        return FastHungarian.FindAssignments(copy);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Large_100x100")]
    public int[] Vanilla_Large_100x100()
    {
        int[,] copy = (int[,])large100X100.Clone();
        return copy.FindAssignments();
    }
    
    [Benchmark]
    [BenchmarkCategory("Large_150x150")]
    public int[] FastHungarian_Large_150x150()
    {
        int[,] copy = (int[,])large150X150.Clone();
        return FastHungarian.FindAssignments(copy);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Large_150x150")]
    public int[] Vanilla_Large_150x150()
    {
        int[,] copy = (int[,])large150X150.Clone();
        return copy.FindAssignments();
    }
    
    [Benchmark]
    [BenchmarkCategory("Rect_50x100")]
    public int[] FastHungarian_Rect_50x100()
    {
        int[,] copy = (int[,])rect50X100.Clone();
        return FastHungarian.FindAssignments(copy);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Rect_50x100")]
    public int[] Vanilla_Rect_50x100()
    {
        int[,] copy = (int[,])rect50X100.Clone();
        return copy.FindAssignments();
    }
    
    [Benchmark]
    [BenchmarkCategory("Rect_100x50")]
    public int[] FastHungarian_Rect_100x50()
    {
        int[,] copy = (int[,])rect100X50.Clone();
        return FastHungarian.FindAssignments(copy);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Rect_100x50")]
    public int[] Vanilla_Rect_100x50()
    {
        int[,] copy = (int[,])rect100X50.Clone();
        return copy.FindAssignments();
    }
    
    [Benchmark]
    [BenchmarkCategory("Sparse_100x100")]
    public int[] FastHungarian_Sparse_100x100()
    {
        int[,] copy = (int[,])sparse100X100.Clone();
        return FastHungarian.FindAssignments(copy);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Sparse_100x100")]
    public int[] Vanilla_Sparse_100x100()
    {
        int[,] copy = (int[,])sparse100X100.Clone();
        return copy.FindAssignments();
    }
    
    [Benchmark]
    [BenchmarkCategory("WideRange_100x100")]
    public int[] FastHungarian_WideRange_100x100()
    {
        int[,] copy = (int[,])wideRange100X100.Clone();
        return FastHungarian.FindAssignments(copy);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("WideRange_100x100")]
    public int[] Vanilla_WideRange_100x100()
    {
        int[,] copy = (int[,])wideRange100X100.Clone();
        return copy.FindAssignments();
    }
}

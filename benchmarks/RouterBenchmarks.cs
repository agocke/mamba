using BenchmarkDotNet.Attributes;
using Mamba;

/// <summary>
/// Microbenchmarks for router matching (no HTTP overhead).
/// </summary>
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
public class RouterBenchmarks
{
    private ReqRouter _smallRouter = null!;
    private ReqRouter _largeRouter = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Small router with 5 routes
        var smallBuilder = new ReqRouter.Builder()
            .MapGet("/", () => "home")
            .MapGet("/about", () => "about")
            .MapGet("/contact", () => "contact")
            .MapGet("/api/users", () => "users")
            .MapGet("/api/posts", () => "posts");
        _smallRouter = smallBuilder.Build();

        // Large router with 100 routes
        var largeBuilder = new ReqRouter.Builder();
        for (int i = 0; i < 100; i++)
        {
            var idx = i;
            largeBuilder.MapGet($"/route{i}", () => $"route{idx}");
        }
        _largeRouter = largeBuilder.Build();
    }

    [Benchmark(Baseline = true)]
    public ReqRouter BuildSmallRouter()
    {
        return new ReqRouter.Builder()
            .MapGet("/", () => "home")
            .MapGet("/about", () => "about")
            .MapGet("/contact", () => "contact")
            .MapGet("/api/users", () => "users")
            .MapGet("/api/posts", () => "posts")
            .Build();
    }

    [Benchmark]
    public ReqRouter BuildLargeRouter()
    {
        var builder = new ReqRouter.Builder();
        for (int i = 0; i < 100; i++)
        {
            var idx = i;
            builder.MapGet($"/route{i}", () => $"route{idx}");
        }
        return builder.Build();
    }
}

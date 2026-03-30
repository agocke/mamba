using System.Diagnostics;
using Mamba;

/// <summary>
/// Measures real-world startup costs that microbenchmarks hide.
/// Run with: dotnet run -c Release --project benchmarks/Mamba.Benchmarks.csproj -- startup
/// </summary>
public static class StartupBenchmark
{
    public static async Task Run()
    {
        Console.WriteLine("=== Mamba Startup Benchmark ===\n");

        // Run multiple iterations to show JIT vs steady-state
        Console.WriteLine("Full startup (router + server + first response):\n");
        
        for (int i = 1; i <= 5; i++)
        {
            var time = await MeasureFullStartup();
            var label = i == 1 ? "(cold - includes JIT)" : "";
            Console.WriteLine($"  Run {i}: {FormatTime(time)} {label}");
        }

        Console.WriteLine("\n--- Component breakdown (already JIT'd) ---\n");
        await MeasureComponents();
    }

    private static async Task<TimeSpan> MeasureFullStartup()
    {
        var sw = Stopwatch.StartNew();

        var router = new ReqRouter.Builder()
            .MapGet("/", () => "Hello, World!")
            .Build();

        var cts = new CancellationTokenSource();
        var port = GetAvailablePort();
        var url = $"http://127.0.0.1:{port}";

        var serverTask = HttpServer.Listen(url, router, options: null, cts.Token);

        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        while (true)
        {
            try
            {
                await client.GetStringAsync($"{url}/");
                break;
            }
            catch (HttpRequestException)
            {
                await Task.Delay(1);
            }
        }

        var elapsed = sw.Elapsed;

        cts.Cancel();
        try { await serverTask; } catch (OperationCanceledException) { }

        return elapsed;
    }

    private static async Task MeasureComponents()
    {
        // Router build (small)
        var sw = Stopwatch.StartNew();
        var router = new ReqRouter.Builder()
            .MapGet("/", () => "OK")
            .MapGet("/hello", () => "Hello!")
            .MapGet("/api/users", () => "users")
            .MapGet("/api/posts", () => "posts")
            .MapGet("/health", () => "healthy")
            .Build();
        Console.WriteLine($"  Router build (5 routes):   {FormatTime(sw.Elapsed)}");

        // Router build (large)
        sw.Restart();
        var largeBuilder = new ReqRouter.Builder();
        for (int i = 0; i < 100; i++)
        {
            var idx = i;
            largeBuilder.MapGet($"/route{i}", () => $"route{idx}");
        }
        largeBuilder.Build();
        Console.WriteLine($"  Router build (100 routes): {FormatTime(sw.Elapsed)}");

        // Server start
        var cts = new CancellationTokenSource();
        var port = GetAvailablePort();
        var url = $"http://127.0.0.1:{port}";

        sw.Restart();
        var serverTask = HttpServer.Listen(url, router, options: null, cts.Token);

        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        while (true)
        {
            try
            {
                await client.GetAsync($"{url}/");
                break;
            }
            catch (HttpRequestException)
            {
                await Task.Delay(1);
            }
        }
        Console.WriteLine($"  Server start (to ready):   {FormatTime(sw.Elapsed)}");

        // Request latencies
        sw.Restart();
        await client.GetStringAsync($"{url}/hello");
        Console.WriteLine($"  Request latency:           {FormatTime(sw.Elapsed)}");

        cts.Cancel();
        try { await serverTask; } catch (OperationCanceledException) { }
    }

    private static string FormatTime(TimeSpan ts)
    {
        if (ts.TotalMilliseconds < 1)
            return $"{ts.TotalMicroseconds:F0} µs";
        if (ts.TotalMilliseconds < 10)
            return $"{ts.TotalMilliseconds:F2} ms";
        if (ts.TotalSeconds < 1)
            return $"{ts.TotalMilliseconds:F1} ms";
        return $"{ts.TotalSeconds:F3} s";
    }

    private static int GetAvailablePort()
    {
        using var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}

using BenchmarkDotNet.Attributes;
using Mamba;

[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
public class HttpBenchmarks
{
    private HttpClient _client = null!;
    private CancellationTokenSource _cts = null!;
    private Task _serverTask = null!;
    private string _baseUrl = null!;

    [GlobalSetup]
    public void Setup()
    {
        var router = new ReqRouter.Builder()
            .MapGet("/", () => "OK")
            .MapGet("/hello", () => "Hello, World!")
            .Build();

        _cts = new CancellationTokenSource();
        var port = GetAvailablePort();
        _baseUrl = $"http://127.0.0.1:{port}";
        
        _serverTask = HttpServer.Listen(_baseUrl, router, options: null, _cts.Token);
        
        _client = new HttpClient();
        
        // Wait for server to be ready
        Thread.Sleep(200);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _client.Dispose();
        _cts.Cancel();
        try { _serverTask.Wait(1000); } catch { }
        _cts.Dispose();
    }

    [Benchmark(Baseline = true)]
    public async Task<string> SimpleGet()
    {
        return await _client.GetStringAsync($"{_baseUrl}/");
    }

    [Benchmark]
    public async Task<string> HelloWorld()
    {
        return await _client.GetStringAsync($"{_baseUrl}/hello");
    }

    [Benchmark]
    public async Task TenSequentialRequests()
    {
        for (int i = 0; i < 10; i++)
        {
            await _client.GetStringAsync($"{_baseUrl}/");
        }
    }

    [Benchmark]
    public async Task TenParallelRequests()
    {
        var tasks = new Task<string>[10];
        for (int i = 0; i < 10; i++)
        {
            tasks[i] = _client.GetStringAsync($"{_baseUrl}/");
        }
        await Task.WhenAll(tasks);
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

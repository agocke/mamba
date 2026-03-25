using System.Diagnostics;
using Mamba;

// Measure from process start
var processStart = Process.GetCurrentProcess().StartTime;
var now = DateTime.Now;
var startupTime = now - processStart;

// Build router
var routerStart = Stopwatch.StartNew();
var router = new ReqRouter.Builder()
    .MapGet("/", () => "OK")
    .MapGet("/ready", () => "1") // Signal ready
    .Build();
var routerTime = routerStart.Elapsed;

// Start server
var cts = new CancellationTokenSource();
var port = args.Length > 0 ? int.Parse(args[0]) : 5000;
var url = $"http://127.0.0.1:{port}";

var serverStart = Stopwatch.StartNew();
var serverTask = HttpServer.Listen(url, router, cts.Token);

// Wait for /ready endpoint  
using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
while (true)
{
    try
    {
        await client.GetStringAsync($"{url}/ready");
        break;
    }
    catch (HttpRequestException)
    {
        await Task.Delay(1);
    }
}
var serverTime = serverStart.Elapsed;

var totalTime = DateTime.Now - processStart;

// Output metrics as simple key=value for parsing
Console.WriteLine($"process_to_main_ms={startupTime.TotalMilliseconds:F2}");
Console.WriteLine($"router_build_ms={routerTime.TotalMilliseconds:F2}");
Console.WriteLine($"server_ready_ms={serverTime.TotalMilliseconds:F2}");
Console.WriteLine($"total_startup_ms={totalTime.TotalMilliseconds:F2}");
Console.WriteLine("READY");

// Keep running until stdin closes (for external measurement)
if (args.Contains("--wait"))
{
    Console.ReadLine();
}

cts.Cancel();

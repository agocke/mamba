using BenchmarkDotNet.Running;

// Check for startup benchmark mode
if (args.Length > 0 && args[0] == "startup")
{
    await StartupBenchmark.Run();
    return;
}

BenchmarkRunner.Run<HttpBenchmarks>();

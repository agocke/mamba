# Mamba

A minimal, AOT-compatible HTTP server for .NET.

## Quick Start

```csharp
var router = new ReqRouter.Builder()
    .MapGet("/hello", () => "Hello, world!")
    .Build();

var server = HttpServer.Start("http://127.0.0.1:5000", router);
await server.WaitForReadyAsync();

Console.WriteLine("Press Ctrl+C to stop");
Console.CancelKeyPress += (_, e) => { e.Cancel = true; server.Stop(); };

await server.ServerTask;
```

## Routing

```csharp
var router = new ReqRouter.Builder()
    // Simple string response
    .MapGet("/", () => "Home")
    
    // Access request data
    .MapGet("/greet", req => $"Hello from {req.Path}")
    
    // Async handler
    .MapGet("/async", async () => await FetchDataAsync())
    
    // Full control over request/response
    .Map("POST", "/api/data", async (req, res) =>
    {
        res.StatusCode = 201;
        res.Headers["X-Custom"] = "value";
        await res.WriteAsync("Created");
    })
    .Build();
```

## Server Lifecycle

```csharp
// Start returns immediately with a handle
var server = HttpServer.Start("http://127.0.0.1:5000", router);

// Wait until server is accepting connections
await server.WaitForReadyAsync();

// Stop gracefully
await server.StopAsync();

// Or use the blocking API
await HttpServer.Listen("http://127.0.0.1:5000", router, cancellationToken);
```

## AOT Publishing

```bash
dotnet publish -c Release
```

The library is fully AOT-compatible with no reflection.

## License

MIT

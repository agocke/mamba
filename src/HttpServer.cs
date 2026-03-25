
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Mamba.Internal;

namespace Mamba;

/// <summary>
/// A minimal HTTP server built on Kestrel.
/// </summary>
public static class HttpServer
{
    /// <summary>
    /// Starts an HTTP server. Returns a handle to check readiness and stop the server.
    /// </summary>
    /// <param name="url">The URL to listen on (e.g., "http://localhost:5000")</param>
    /// <param name="router">The router to handle incoming requests</param>
    /// <param name="options">Optional server configuration</param>
    public static ServerHandle Start(string url, ReqRouter router, ServerOptions? options = null)
    {
        var cts = new CancellationTokenSource();
        var readyTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var serverTask = RunServerAsync(url, router, options ?? new ServerOptions(), readyTcs, cts.Token);
        return new ServerHandle(serverTask, readyTcs, cts);
    }

    /// <summary>
    /// Starts an HTTP server and blocks until the cancellation token is triggered.
    /// </summary>
    /// <param name="url">The URL to listen on (e.g., "http://localhost:5000")</param>
    /// <param name="router">The router to handle incoming requests</param>
    /// <param name="options">Optional server configuration</param>
    /// <param name="cancellationToken">Token to stop the server</param>
    public static async Task Listen(string url, ReqRouter router, ServerOptions? options = null, CancellationToken cancellationToken = default)
    {
        var readyTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        await RunServerAsync(url, router, options ?? new ServerOptions(), readyTcs, cancellationToken).ConfigureAwait(false);
    }

    private static async Task RunServerAsync(
        string url,
        ReqRouter router,
        ServerOptions options,
        TaskCompletionSource readyTcs,
        CancellationToken cancellationToken)
    {
        var uri = new Uri(url);
        var host = uri.Host;
        var port = uri.Port;

        var loggerFactory = NullLoggerFactory.Instance;

        var kestrelOptions = new KestrelServerOptions();
        kestrelOptions.Listen(System.Net.IPAddress.Parse(host == "localhost" ? "127.0.0.1" : host), port);

        // Apply timeout and limit settings
        kestrelOptions.Limits.KeepAliveTimeout = options.KeepAliveTimeout;
        kestrelOptions.Limits.RequestHeadersTimeout = options.RequestTimeout;
        kestrelOptions.Limits.MaxRequestBodySize = options.MaxRequestBodySize;

        var socketTransportFactory = new SocketTransportFactory(
            Options.Create(new SocketTransportOptions()),
            loggerFactory);

        var server = new KestrelServer(
            Options.Create(kestrelOptions),
            socketTransportFactory,
            loggerFactory);

        try
        {
            await server.StartAsync(new KestrelBridge(router), cancellationToken).ConfigureAwait(false);
            readyTcs.TrySetResult();

            await Task.Delay(Timeout.Infinite, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested
        }
        finally
        {
            await server.StopAsync(CancellationToken.None).ConfigureAwait(false);
            server.Dispose();
        }
    }
}
namespace Mamba;

/// <summary>
/// Configuration options for the HTTP server.
/// </summary>
public sealed class ServerOptions
{
    /// <summary>
    /// Maximum time allowed for reading the entire request (headers + body).
    /// Default: 30 seconds.
    /// </summary>
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Maximum time a connection can remain idle (keep-alive).
    /// Default: 2 minutes.
    /// </summary>
    public TimeSpan KeepAliveTimeout { get; set; } = TimeSpan.FromMinutes(2);

    /// <summary>
    /// Maximum size of the request body in bytes.
    /// Default: 30 MB.
    /// </summary>
    public long MaxRequestBodySize { get; set; } = 30 * 1024 * 1024;
}

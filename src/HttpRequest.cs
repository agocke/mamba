
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Mamba;

/// <summary>
/// Represents an incoming HTTP request.
/// </summary>
public sealed class HttpRequest
{
    private readonly IHttpRequestFeature _request;

    internal HttpRequest(IHttpRequestFeature request, CancellationToken cancellationToken)
    {
        _request = request;
        CancellationToken = cancellationToken;
    }

    public string Method => _request.Method;
    public string Path => _request.Path;
    public string QueryString => _request.QueryString;
    public IHeaderDictionary Headers => _request.Headers;
    public Stream Body => _request.Body;

    /// <summary>
    /// Triggered when the client disconnects or the request times out.
    /// </summary>
    public CancellationToken CancellationToken { get; }
}
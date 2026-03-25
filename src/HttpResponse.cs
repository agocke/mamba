using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Mamba;

/// <summary>
/// Represents an outgoing HTTP response.
/// </summary>
public sealed class HttpResponse
{
    private readonly IHttpResponseFeature _response;
    private readonly IHttpResponseBodyFeature _body;
    private bool _headersSent;

    internal HttpResponse(IHttpResponseFeature response, IHttpResponseBodyFeature body)
    {
        _response = response;
        _body = body;
    }

    /// <summary>
    /// True if headers have been sent to the client.
    /// After this, StatusCode and Headers cannot be modified.
    /// </summary>
    public bool HeadersSent => _headersSent;

    /// <summary>
    /// Gets or sets the status code. Must be set before writing to Body.
    /// </summary>
    public int StatusCode
    {
        get => _response.StatusCode;
        set
        {
            ThrowIfHeadersSent();
            _response.StatusCode = value;
        }
    }

    /// <summary>
    /// Response headers. Must be set before writing to Body.
    /// </summary>
    public IHeaderDictionary Headers
    {
        get => _response.Headers;
    }

    /// <summary>
    /// The response body stream. Writing to this sends the headers.
    /// </summary>
    public Stream Body => _body.Stream;

    /// <summary>
    /// Writes a string to the response body as UTF-8.
    /// This sends the headers if not already sent.
    /// </summary>
    public async Task WriteAsync(string content, CancellationToken cancellationToken = default)
    {
        _headersSent = true;
        var bytes = System.Text.Encoding.UTF8.GetBytes(content);
        await Body.WriteAsync(bytes, cancellationToken).ConfigureAwait(false);
    }

    private void ThrowIfHeadersSent()
    {
        if (_headersSent)
        {
            throw new InvalidOperationException("Cannot modify response after headers have been sent.");
        }
    }
}

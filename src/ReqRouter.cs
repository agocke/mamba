
namespace Mamba;

/// <summary>
/// An immutable request router that matches incoming requests to handlers.
/// </summary>
public sealed partial class ReqRouter
{
    private readonly Dictionary<RouteKey, Func<HttpRequest, HttpResponse, Task>> _routes;

    private ReqRouter(Dictionary<RouteKey, Func<HttpRequest, HttpResponse, Task>> routes)
    {
        _routes = routes;
    }

    internal async Task RouteAsync(HttpRequest request, HttpResponse response)
    {
        var key = new RouteKey(request.Method, request.Path);
        
        if (_routes.TryGetValue(key, out var handler))
        {
            await handler(request, response).ConfigureAwait(false);
        }
        else
        {
            response.StatusCode = 404;
            await response.WriteAsync("Not Found").ConfigureAwait(false);
        }
    }

    private readonly record struct RouteKey(string Method, string Path);
}

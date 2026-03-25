
namespace Mamba;

public sealed partial class ReqRouter
{
    public sealed class Builder
    {
        private readonly Dictionary<RouteKey, Func<HttpRequest, HttpResponse, Task>> _routes = new();

        /// <summary>
        /// Maps a route to a handler that receives request and response objects.
        /// </summary>
        public Builder Map(string method, string path, Func<HttpRequest, HttpResponse, Task> handler)
        {
            _routes[new RouteKey(method.ToUpperInvariant(), path)] = handler;
            return this;
        }

        /// <summary>
        /// Maps a GET route that returns a string response.
        /// </summary>
        public Builder MapGet(string path, Func<string> handler)
        {
            return Map("GET", path, async (_, response) =>
            {
                response.StatusCode = 200;
                response.Headers["Content-Type"] = "text/plain; charset=utf-8";
                await response.WriteAsync(handler()).ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Maps a GET route with an async handler that returns a string response.
        /// </summary>
        public Builder MapGet(string path, Func<Task<string>> handler)
        {
            return Map("GET", path, async (_, response) =>
            {
                response.StatusCode = 200;
                response.Headers["Content-Type"] = "text/plain; charset=utf-8";
                await response.WriteAsync(await handler().ConfigureAwait(false)).ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Maps a GET route that receives the request and returns a string response.
        /// </summary>
        public Builder MapGet(string path, Func<HttpRequest, string> handler)
        {
            return Map("GET", path, async (request, response) =>
            {
                response.StatusCode = 200;
                response.Headers["Content-Type"] = "text/plain; charset=utf-8";
                await response.WriteAsync(handler(request)).ConfigureAwait(false);
            });
        }

        public ReqRouter Build() => new ReqRouter(new Dictionary<RouteKey, Func<HttpRequest, HttpResponse, Task>>(_routes));
    }
}
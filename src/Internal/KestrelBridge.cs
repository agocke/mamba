// =============================================================================
// KESTREL BRIDGE - All IFeatureCollection ugliness quarantined here
// =============================================================================
// This file contains the minimal glue needed to bridge Kestrel's internal
// abstractions to Mamba's clean public API. Do not add IFeatureCollection
// or related types anywhere else in the codebase.
// =============================================================================

using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;

namespace Mamba.Internal;

/// <summary>
/// Bridges Kestrel's IHttpApplication to Mamba's router.
/// </summary>
internal sealed class KestrelBridge : IHttpApplication<KestrelBridge.Context>
{
    private readonly ReqRouter _router;

    public KestrelBridge(ReqRouter router) => _router = router;

    // === THE ONLY PLACE IFeatureCollection IS TOUCHED ===
    public Context CreateContext(IFeatureCollection features)
    {
        var requestLifetime = features.Get<IHttpRequestLifetimeFeature>();
        var cancellationToken = requestLifetime?.RequestAborted ?? CancellationToken.None;

        return new Context(
            new HttpRequest(features.Get<IHttpRequestFeature>()!, cancellationToken),
            new HttpResponse(
                features.Get<IHttpResponseFeature>()!,
                features.Get<IHttpResponseBodyFeature>()!));
    }

    public Task ProcessRequestAsync(Context context)
        => _router.RouteAsync(context.Request, context.Response);

    public void DisposeContext(Context context, Exception? exception) { }

    internal readonly struct Context
    {
        public readonly HttpRequest Request;
        public readonly HttpResponse Response;

        public Context(HttpRequest request, HttpResponse response)
        {
            Request = request;
            Response = response;
        }
    }
}

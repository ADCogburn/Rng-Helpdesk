using RngHelpdesk.Contracts.Security;

namespace RngHelpdesk.Api.Security;

/// <summary>
/// Abstraction to retrieve the current request context, or return an Anonymous (unauthenticated) context if none is available.
/// </summary>
public sealed class HttpRequestContextAccessor : IRequestContextAccessor
{
    public IRequestContext Context { get; }

    public HttpRequestContextAccessor(
        IHttpContextAccessor httpContextAccessor,
        IRequestContextFactory contextFactory)
    {
        var httpContext = httpContextAccessor.HttpContext;

        Context = httpContext is null
            ? new AnonymousRequestContext() // still allows "no auth required" commands, if there ever are any.
            : contextFactory.CreateHttpContext(httpContext);
    }
}
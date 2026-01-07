using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Operations.Security;
using System.Security.Claims;

/// <summary>
/// Creates an HTTP request context for the API.
/// </summary>
public sealed class HttpRequestContextFactory : IRequestContextFactory
{
    private readonly AuthorizationService _authService;

    public HttpRequestContextFactory(AuthorizationService authService)
    {
        _authService = authService;
    }

    public IRequestContext CreateHttpContext(HttpContext httpContext)
    {
        var user = httpContext.User;

        if (user?.Identity?.IsAuthenticated != true)
            return new AnonymousRequestContext();

        var actorId = Guid.Parse(
            user.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        var baseContext = new RequestContext
        {
            ActorId = actorId,
            ActorType = ActorType.WebUser,
            IsAuthenticated = true
        };

        var authority = _authService.ResolveAuthority(baseContext);

        return new RequestContext
        {
            ActorId = actorId,
            ActorType = ActorType.WebUser,
            IsAuthenticated = true,
            AuthorityRole = authority
        };
    }
}

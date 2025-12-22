using RngHelpdesk.Contracts.Security;
using System.Security.Claims;

/// <summary>
/// Creates an HTTP request context for the API.
/// </summary>
public sealed class HttpRequestContextFactory : IRequestContextFactory
{
    public IRequestContext CreateHttpContext(HttpContext httpContext)
    {
        var user = httpContext.User;

        if (user?.Identity?.IsAuthenticated != true)
            return new AnonymousRequestContext();

        var actorId = Guid.Parse(
            user.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        var roles = user.FindAll(ClaimTypes.Role)
                        .Select(r => r.Value)
                        .ToHashSet();

        return new RequestContext
        {
            ActorId = actorId,
            ActorType = ActorType.WebUser,
            Roles = roles,
            Claims = user.Claims.Select(c => c.Type).ToHashSet(),
            IsAuthenticated = true,
            IsMember = roles.Contains("Member") || roles.Contains("Admin")
        };
    }
}
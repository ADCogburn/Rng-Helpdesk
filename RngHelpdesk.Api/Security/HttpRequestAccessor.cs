using RngHelpdesk.Contracts.Security;
using System.Security.Claims;

namespace RngHelpdesk.Api.Security;

/// <summary>
/// Abstraction to retrieve the current request context, or return an Anonymous (unauthenticated) context if none is available.
/// </summary>
public sealed class HttpRequestContextAccessor : IRequestContextAccessor
{
    public IRequestContext Context { get; }

    public HttpRequestContextAccessor(
        IHttpContextAccessor httpContextAccessor)
    {
        var user = httpContextAccessor.HttpContext?.User;

        var userIdClaim = user?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
            throw new InvalidOperationException("No authenticated user id exists for this request.");

        Context = new RequestContext
        {
            UserId = userId
        };
    }
}
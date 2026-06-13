using System.Security.Claims;

namespace RngHelpdesk.Api.Helpers;

public static class ClaimsPrincipalExtensions
{
    public static ulong GetUserId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

        if (userIdClaim == null || !ulong.TryParse(userIdClaim.Value, out var userId))
            throw new InvalidOperationException("User ID claim is missing or invalid.");

        return userId;
    }
}

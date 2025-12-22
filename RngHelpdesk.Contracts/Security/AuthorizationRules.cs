using RngHelpdesk.Contracts.Security;

public static class AuthorizationRules
{
    /// <summary>
    /// Requires Authentication - should always be "true" from the API, but this is incase the DiscordBot fails on this.
    /// </summary>
    /// <param name="context"></param>
    /// <exception cref="NotAuthenticatedException"></exception>
    public static void RequireAuthentication(this IRequestContext context)
    {
        if (!context.IsAuthenticated || context.ActorId == Guid.Empty || context.ActorType == ActorType.Unknown)
            throw new UnauthorizedAccessException();
    }

    /// <summary>
    /// Entity making this request is required to be a member of the clan.
    /// </summary>
    /// <param name="context"></param>
    /// <exception cref="NotAuthorizedException"></exception>
    public static void RequireMember(this IRequestContext context)
    {
        if (!context.IsMember)
            throw new UnauthorizedAccessException();
    }

    /// <summary>
    /// User is required to have the specified role given as an argument.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="role"></param>
    /// <exception cref="UnauthorizedAccessException"></exception>
    public static void RequireRole(this IRequestContext context, SystemRoles role)
    {
        if (!context.HasRole(role))
            throw new UnauthorizedAccessException(
                $"Required role '{role}' was not present."
            );
    }
}
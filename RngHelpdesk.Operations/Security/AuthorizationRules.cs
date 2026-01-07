using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Domain.Users;

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
    /// User is required to have the specified role given as an argument.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="role"></param>
    /// <exception cref="UnauthorizedAccessException"></exception>
    public static void RequireRole(this IRequestContext context, AuthorityRole role)
    {
        if (!context.IsRole(role))
            throw new UnauthorizedAccessException(
                $"Required role '{role}' was not present."
            );
    }

    /// <summary>
    /// Requires any level of Admin role.
    /// </summary>
    /// <param name="context"></param>
    /// <exception cref="UnauthorizedAccessException"></exception>
    public static void RequireAdminRole(this IRequestContext context)
    {
        if (!context.IsRole(AuthorityRole.Administrator) && !context.IsRole(AuthorityRole.SuperAdministrator) && !context.IsRole(AuthorityRole.Owner))
            throw new UnauthorizedAccessException(
                $"Required role 'Owner' or 'Administrator' was not present."
            );
    }

    /// <summary>
    /// Requires SuperAdmin (i.e. "DeputyOwner") or Owner auth role.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="role"></param>
    /// <exception cref="UnauthorizedAccessException"></exception>
    public static void RequireSuperAdminRole(this IRequestContext context)
    {
        if (!context.IsRole(AuthorityRole.SuperAdministrator) && !context.IsRole(AuthorityRole.Owner))
            throw new UnauthorizedAccessException(
                $"Required role 'Owner' or 'Administrator' was not present."
            );
    }

    /// <summary>
    /// Requires that the request is made manually (i.e. not by a bot).
    /// </summary>
    /// <param name="context"></param>
    /// <exception cref="UnauthorizedAccessException"></exception>
    public static void RequireNonBot(this IRequestContext context)
    {
        if (context.ActorType == ActorType.Bot)
            throw new UnauthorizedAccessException("Bots are not authorized to perform this action.");
    }

    /// <summary>
    /// Requires that the request is made by the system (i.e. a bot).
    /// </summary>
    /// <param name="context"></param>
    /// <exception cref="UnauthorizedAccessException"></exception>
    public static void RequireBot(this IRequestContext context)
    {
        if (context.ActorType != ActorType.Bot)
            throw new UnauthorizedAccessException("Only bots are authorized to perform this action.");
    }
}
namespace RngHelpdesk.Contracts.Security;

public static class AuthorizationRulesExtension
{
    public static bool HasRole(this IRequestContext ctx, SystemRoles role)
        => ctx.Roles.Contains(role.ToString());

    public static bool IsBot(this IRequestContext ctx)
        => ctx.ActorType == ActorType.Bot;
}
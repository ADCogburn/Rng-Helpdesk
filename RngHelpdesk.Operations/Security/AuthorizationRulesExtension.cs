using RngHelpdesk.Domain.Users;

namespace RngHelpdesk.Contracts.Security;

public static class AuthorizationRulesExtension
{
    public static bool IsRole(this IRequestContext ctx, AuthorityRole role)
        => ctx.AuthorityRole == role;

    public static bool IsBot(this IRequestContext ctx)
        => ctx.ActorType == ActorType.Bot;
}
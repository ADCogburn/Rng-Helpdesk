namespace RngHelpdesk.Api.Security;

public static class AuthPolicies
{
    public const string AdminPlus = nameof(AdminPlus);
    public const string OwnerOnly = nameof(OwnerOnly);
    public const string DiscordBotOnly = nameof(DiscordBotOnly);
}

using RngHelpdesk.Domain.Users;

namespace RngHelpdesk.Operations.Tests;

internal static class TestUsers
{
    public const ulong DefaultActingUserId = 1;
    public const ulong DefaultDiscordId = 100;
    public const string DefaultDiscordUsername = "discordUser";

    public static DiscordAccount ValidDiscordAccount(ulong discordId = DefaultDiscordId, string username = DefaultDiscordUsername)
        => new(discordId, username);
}

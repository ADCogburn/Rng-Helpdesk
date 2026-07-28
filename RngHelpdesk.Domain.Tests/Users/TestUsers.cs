using RngHelpdesk.Domain.Users;

namespace RngHelpdesk.Domain.Tests.Users;

internal static class TestUsers
{
    public const ulong DefaultActingUserId = 1;
    public const ulong DefaultDiscordId = 100;
    public const string DefaultDiscordUsername = "discordUser";

    public static DiscordAccount ValidDiscordAccount(ulong discordId = DefaultDiscordId, string username = DefaultDiscordUsername)
        => new(discordId, username);

    /// <summary>
    /// Creates a valid User via the public API and clears the resulting UserCreatedEvent,
    /// so callers can assert UncommittedDomainEvents against only the behavior under test.
    /// </summary>
    public static User CreateValidUser(
        ulong actingUserId = DefaultActingUserId,
        ulong discordId = DefaultDiscordId,
        string discordUsername = DefaultDiscordUsername,
        IEnumerable<RunescapeAccount>? runescapeAccounts = null)
    {
        var user = User.Create(actingUserId, ValidDiscordAccount(discordId, discordUsername), runescapeAccounts ?? Array.Empty<RunescapeAccount>());
        user.ClearUncommittedDomainEvents();
        return user;
    }
}

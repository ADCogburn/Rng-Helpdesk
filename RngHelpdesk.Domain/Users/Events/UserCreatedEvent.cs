using RngHelpdesk.Domain.Common;
using System.Text.Json.Serialization;

namespace RngHelpdesk.Domain.Users.Events;

public sealed class UserCreatedEvent : IDomainEvent
{
    public int UserId { get; }
    public AuthorityRole AuthorityRole { get; }
    public IReadOnlyList<DiscordAccount> DiscordAccounts { get; }
    public IReadOnlyList<RunescapeAccount> RunescapeAccounts { get; }
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    [JsonConstructor]
    public UserCreatedEvent(
        int userId,
        AuthorityRole authorityRole,
        IReadOnlyList<DiscordAccount> discordAccounts,
        IReadOnlyList<RunescapeAccount> runescapeAccounts)
    {
        UserId = userId;
        AuthorityRole = authorityRole;
        DiscordAccounts = discordAccounts ?? [];
        RunescapeAccounts = runescapeAccounts ?? [];
    }

    /// <summary>
    /// Domain factory for creating the event (not used by JSON deserialization).
    /// </summary>
    public UserCreatedEvent(
        int userId,
        AuthorityRole authorityRole,
        IEnumerable<DiscordAccount> discordAccounts,
        IEnumerable<RunescapeAccount> runescapeAccounts)
        : this(userId, authorityRole, discordAccounts.ToList(), runescapeAccounts.ToList())
    {
    }
}

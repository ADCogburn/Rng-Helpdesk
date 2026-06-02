using RngHelpdesk.Domain.Common;
using System.Text.Json.Serialization;

namespace RngHelpdesk.Domain.Users.Events;

public sealed class UserCreatedEvent : AuditableEvent
{
    public int UserId { get; }
    public IReadOnlyList<DiscordAccount> DiscordAccounts { get; }
    public IReadOnlyList<RunescapeAccount> RunescapeAccounts { get; }

    [JsonConstructor]
    public UserCreatedEvent(
        int userId,
        int actingUserId,
        IEnumerable<DiscordAccount> discordAccounts,
        IEnumerable<RunescapeAccount> runescapeAccounts,
        DateTimeOffset occurredAt) : base(actingUserId, occurredAt)
    {
        UserId = userId;
        DiscordAccounts = discordAccounts.ToList();
        RunescapeAccounts = runescapeAccounts.ToList();
    }

    /// <summary>
    /// Domain factory for creating the event (not used by JSON deserialization).
    /// </summary>
    public UserCreatedEvent(
        int userId,
        int actingUserId,
        IEnumerable<DiscordAccount> discordAccounts,
        IEnumerable<RunescapeAccount> runescapeAccounts)
        : this(userId, actingUserId, discordAccounts, runescapeAccounts, DateTimeOffset.UtcNow)
    {
    }
}

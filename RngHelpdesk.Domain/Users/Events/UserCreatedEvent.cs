using RngHelpdesk.Domain.Common;
using System.Text.Json.Serialization;

namespace RngHelpdesk.Domain.Users.Events;

public sealed class UserCreatedEvent : IDomainEvent
{
    // Audting properties
    public ulong ActingUserId { get; }
    public DateTimeOffset OccurredAt { get; }

    public ulong UserId { get; }
    public DiscordAccount DiscordAccount { get; }
    public IReadOnlyList<RunescapeAccount> RunescapeAccounts { get; }

    /// <summary>
    /// Constructor that allows for JSON Serialization when rehydrating from the event store.
    /// Other code should call Create below for domain event creation, not this constructor.
    /// </summary>
    /// <param name="actingUserId"></param>
    /// <param name="occurredAt"></param>
    /// <param name="discordAccount"></param>
    /// <param name="runescapeAccounts"></param>
    [JsonConstructor]
    public UserCreatedEvent(
        ulong actingUserId,
        DateTimeOffset occurredAt,
        DiscordAccount discordAccount,
        IReadOnlyList<RunescapeAccount> runescapeAccounts)
    {
        ActingUserId = actingUserId;
        OccurredAt = occurredAt;

        UserId = discordAccount.DiscordId;
        DiscordAccount = discordAccount;
        RunescapeAccounts = runescapeAccounts;
    }

    /// <summary>
    /// Domain factory for creating the event (not used by JSON deserialization).
    /// </summary>
    public static UserCreatedEvent Create(
        ulong actingUserId,
        DiscordAccount discordAccount,
        IEnumerable<RunescapeAccount> runescapeAccounts)
    {
        return new UserCreatedEvent(
            actingUserId: actingUserId,
            occurredAt: DateTimeOffset.UtcNow,
            discordAccount: discordAccount,
            runescapeAccounts: runescapeAccounts.ToList());
    }
}

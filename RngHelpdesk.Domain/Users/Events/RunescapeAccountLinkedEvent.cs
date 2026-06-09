using RngHelpdesk.Domain.Common;
using System.Text.Json.Serialization;

namespace RngHelpdesk.Domain.Users.Events;

public sealed class RunescapeAccountLinkedEvent : IDomainEvent
{
    // Audting properties
    public ulong ActingUserId { get; }
    public DateTimeOffset OccurredAt { get; }

    public ulong UserId { get; }
    public string Username { get; }

    [JsonConstructor]
    public RunescapeAccountLinkedEvent(ulong actingUserId, ulong userId, DateTimeOffset occurredAt, string username)
    {
        ActingUserId = actingUserId;
        OccurredAt = occurredAt;

        UserId = userId;
        Username = username;
    }

    public static RunescapeAccountLinkedEvent Create(ulong actingUserId, ulong userId, string username)
    {
        return new RunescapeAccountLinkedEvent(
            actingUserId: actingUserId,
            userId: userId,
            occurredAt: DateTimeOffset.UtcNow,
            username: username);
    }
}


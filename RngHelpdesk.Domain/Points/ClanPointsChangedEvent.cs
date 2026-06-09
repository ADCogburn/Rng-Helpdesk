using RngHelpdesk.Domain.Common;
using System.Text.Json.Serialization;

/// <summary>
/// Domain event representing a change in clan points for a user.
/// </summary>
public sealed class ClanPointsChangedEvent : IDomainEvent
{
    // Auditing properties
    public ulong ActingUserId { get; }
    public DateTimeOffset OccurredAt { get; }

    public ulong UserId { get; }
    public int Delta { get; }
    public string Reason { get; }


    [JsonConstructor]
    public ClanPointsChangedEvent(ulong actingUserId, DateTimeOffset occurredAt, ulong userId, int delta, string reason)
    {
        ActingUserId = actingUserId;
        OccurredAt = occurredAt;

        UserId = userId;
        Delta = delta;
        Reason = reason;
    }

    public static ClanPointsChangedEvent Create(ulong actingUserId, ulong userId, int delta, string reason)
    {
        return new ClanPointsChangedEvent(
            actingUserId: actingUserId,
            occurredAt: DateTimeOffset.UtcNow,
            userId: userId,
            delta: delta,
            reason: reason);
    }
}

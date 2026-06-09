using RngHelpdesk.Domain.Common;
using System.Text.Json.Serialization;

namespace RngHelpdesk.Domain.Users.Events;

public sealed class UserDeactivatedEvent : IDomainEvent
{
    // Audting properties
    public ulong ActingUserId { get; }
    public DateTimeOffset OccurredAt { get; }

    public ulong UserId { get; }

    [JsonConstructor]
    public UserDeactivatedEvent(ulong actingUserId, DateTimeOffset occurredAt, ulong userId)
    {
        UserId = userId;
        ActingUserId = actingUserId;
        OccurredAt = occurredAt;
    }

    public static UserDeactivatedEvent Create(ulong actingUserId, ulong userId)
    {
        return new UserDeactivatedEvent(
            actingUserId: actingUserId,
            occurredAt: DateTimeOffset.UtcNow,
            userId: userId);
    }
}
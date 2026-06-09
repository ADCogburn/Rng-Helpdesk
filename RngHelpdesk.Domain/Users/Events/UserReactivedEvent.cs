using RngHelpdesk.Domain.Common;
using System.Text.Json.Serialization;

namespace RngHelpdesk.Domain.Users.Events;

public sealed class UserReactivatedEvent : IDomainEvent
{
    // Auditing properties
    public DateTimeOffset OccurredAt { get; }

    public ulong ActingUserId { get; }


    public ulong UserId { get; }

    [JsonConstructor]
    public UserReactivatedEvent(ulong actingUserId, DateTimeOffset occurredAt, ulong userId)
    {
        UserId = userId;
        ActingUserId = actingUserId;
        OccurredAt = occurredAt;
    }

    public static UserReactivatedEvent Create(ulong actingUserId, ulong userId)
    {
        return new UserReactivatedEvent(
            actingUserId: actingUserId,
            occurredAt: DateTimeOffset.UtcNow,
            userId: userId);
    }
}
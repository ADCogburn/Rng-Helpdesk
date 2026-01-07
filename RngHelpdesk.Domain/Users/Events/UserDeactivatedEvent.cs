using RngHelpdesk.Domain.Common;

namespace RngHelpdesk.Domain.Users.Events;

public sealed class UserDeactivatedEvent : IDomainEvent
{
    public int UserId { get; }
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    public UserDeactivatedEvent(int userId)
    {
        UserId = userId;
    }
}
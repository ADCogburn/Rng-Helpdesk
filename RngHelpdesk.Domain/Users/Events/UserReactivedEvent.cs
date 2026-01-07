using RngHelpdesk.Domain.Common;

namespace RngHelpdesk.Domain.Users.Events;

public sealed class UserReactivatedEvent : IDomainEvent
{
    public int UserId { get; }
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    public UserReactivatedEvent(int userId)
    {
        UserId = userId;
    }
}
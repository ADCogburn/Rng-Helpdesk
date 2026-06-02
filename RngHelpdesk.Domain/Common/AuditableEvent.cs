namespace RngHelpdesk.Domain.Common;

public abstract class AuditableEvent : IDomainEvent, IAuditableEvent
{
    public int ActingUserId { get; }
    public DateTimeOffset OccurredAt { get; }

    protected AuditableEvent(
        int actingUserId,
        DateTimeOffset occurredAt)
    {
        ActingUserId = actingUserId;
        OccurredAt = occurredAt;
    }
}

namespace RngHelpdesk.Domain.Common;

public interface IAuditableEvent
{
    int ActingUserId { get; }
    DateTimeOffset OccurredAt { get; }
}

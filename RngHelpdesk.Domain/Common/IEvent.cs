namespace RngHelpdesk.Domain.Common;

public interface IEvent
{
    ulong ActingUserId { get; }
    DateTimeOffset OccurredAt { get; }
}

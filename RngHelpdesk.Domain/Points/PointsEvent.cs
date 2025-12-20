using RngHelpdesk.Domain.Common;

public sealed class PointsEvent
{
    public int UserId { get; }
    public int Delta { get; }
    public string Reason { get; }
    public DateTime OccurredAt { get; }

    public PointsEvent(int userId, int delta, string reason, DateTime occurredAt)
    {
        if (delta == 0)
            throw new DomainException("Points event must change points.");

        UserId = userId;
        Delta = delta;
        Reason = reason;
        OccurredAt = occurredAt;
    }
}

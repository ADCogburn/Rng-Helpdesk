using RngHelpdesk.Domain.Common;

/// <summary>
/// Domain event representing a change in clan points for a user.
/// </summary>
public sealed class ClanPointsChangedEvent : IDomainEvent
{
    public Guid Id { get; }
    public int UserId { get; }
    public int Delta { get; }
    public string Reason { get; }
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    public ClanPointsChangedEvent(int userId, int delta, string reason)
    {
        if (delta == 0)
            throw new DomainException("Points event must change points.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Reason for points change must be provided.");

        Id = Guid.NewGuid();
        UserId = userId;
        Delta = delta;
        Reason = reason;
    }
}

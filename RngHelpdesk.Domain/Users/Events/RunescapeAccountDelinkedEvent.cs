using RngHelpdesk.Domain.Common;

namespace RngHelpdesk.Domain.Users.Events;

public sealed class RunescapeAccountDelinkedEvent : IDomainEvent
{
    public int UserId { get; }
    public string Username { get; }
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    public RunescapeAccountDelinkedEvent(int userId, string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new DomainException("Runescape username must be provided.");

        UserId = userId;
        Username = username;
    }
}
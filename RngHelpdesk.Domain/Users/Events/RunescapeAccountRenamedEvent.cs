using RngHelpdesk.Domain.Common;
namespace RngHelpdesk.Domain.Users.Events;

public sealed class RunescapeAccountRenamedEvent : IDomainEvent
{
    public int UserId { get; }
    public string OldUsername { get; }
    public string NewUsername { get; }
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    public RunescapeAccountRenamedEvent(
        int userId,
        string oldUsername,
        string newUsername)
    {
        if (string.IsNullOrWhiteSpace(oldUsername))
            throw new DomainException("Old username required.");

        if (string.IsNullOrWhiteSpace(newUsername))
            throw new DomainException("New username required.");

        UserId = userId;
        OldUsername = oldUsername;
        NewUsername = newUsername;
    }
}
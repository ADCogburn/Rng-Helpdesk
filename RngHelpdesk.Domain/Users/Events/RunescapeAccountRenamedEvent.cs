using RngHelpdesk.Domain.Common;
namespace RngHelpdesk.Domain.Users.Events;

public sealed class RunescapeAccountRenamedEvent : IDomainEvent
{
    // Auditing properties
    public ulong ActingUserId { get; }
    public DateTimeOffset OccurredAt { get; }

    public ulong UserId { get; }
    public string OldUsername { get; }
    public string NewUsername { get; }

    public RunescapeAccountRenamedEvent(
        ulong actingUserId,
        DateTimeOffset occuredAt,
        ulong userId,
        string oldUsername,
        string newUsername)
    {
        ActingUserId = actingUserId;
        OccurredAt = occuredAt;

        UserId = userId;
        OldUsername = oldUsername;
        NewUsername = newUsername;
    }

    public static RunescapeAccountRenamedEvent Create(ulong actingUserId, ulong userId, string oldUsername, string newUsername)
    {
        return new RunescapeAccountRenamedEvent(
            actingUserId: actingUserId,
            occuredAt: DateTimeOffset.UtcNow,
            userId: userId,
            oldUsername: oldUsername,
            newUsername: newUsername);
    }
}
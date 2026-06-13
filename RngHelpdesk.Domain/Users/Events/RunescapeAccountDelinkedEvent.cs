using RngHelpdesk.Domain.Common;

namespace RngHelpdesk.Domain.Users.Events;

public sealed class RunescapeAccountDelinkedEvent : IDomainEvent
{
    // Audting properties
    public ulong ActingUserId { get; }
    public DateTimeOffset OccurredAt { get; }

    public ulong UserId { get; }
    public string Username { get; }

    public RunescapeAccountDelinkedEvent(ulong actingUserId, DateTimeOffset occuredAt, ulong userId, string username)
    {
        ActingUserId = actingUserId;
        OccurredAt = occuredAt;

        UserId = userId;
        Username = username;
    }

    public static RunescapeAccountDelinkedEvent Create(ulong actingUserId, ulong userId, string username)
    {
        return new RunescapeAccountDelinkedEvent(
            actingUserId: actingUserId,
            occuredAt: DateTimeOffset.UtcNow,
            userId: userId,
            username: username);
    }
}
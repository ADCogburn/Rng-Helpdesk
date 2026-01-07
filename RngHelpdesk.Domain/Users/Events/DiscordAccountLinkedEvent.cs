using RngHelpdesk.Domain.Common;

namespace RngHelpdesk.Domain.Users.Events;

public sealed class DiscordAccountLinkedEvent : IDomainEvent
{
    public int UserId { get; }
    public ulong DiscordId { get; }
    public string Username { get; }
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    public DiscordAccountLinkedEvent(
        int userId,
        ulong discordId,
        string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new DomainException("Discord username must be provided.");

        UserId = userId;
        DiscordId = discordId;
        Username = username;
    }
}


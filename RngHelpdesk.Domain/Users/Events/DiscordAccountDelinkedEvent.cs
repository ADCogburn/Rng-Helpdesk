using RngHelpdesk.Domain.Common;

namespace RngHelpdesk.Domain.Users.Events;

public sealed class DiscordAccountDelinkedEvent : IDomainEvent
{
    public int UserId { get; }
    public ulong DiscordId { get; }
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    public DiscordAccountDelinkedEvent(int userId, ulong discordId)
    {
        UserId = userId;
        DiscordId = discordId;
    }
}

using RngHelpdesk.Domain.Users.Events;
using RngHelpdesk.Infrastructure.Common;

namespace RngHelpdesk.Infrastructure.Users;

public sealed class DiscordAccountHistoryProjection :
    IProjectionHandler<DiscordAccountLinkedEvent>,
    IProjectionHandler<DiscordAccountDelinkedEvent>
{
    private readonly Dictionary<int, List<ulong>> _history = new();

    public void Project(DiscordAccountLinkedEvent e)
    {
        _history.TryAdd(e.UserId, new List<ulong>());
        _history[e.UserId].Add(e.DiscordId);
    }

    public void Project(DiscordAccountDelinkedEvent e)
    {
        _history.TryAdd(e.UserId, new List<ulong>());
        _history[e.UserId].Add(e.DiscordId);
    }
}
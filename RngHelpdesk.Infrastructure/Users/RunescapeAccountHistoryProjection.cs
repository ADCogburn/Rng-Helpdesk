using RngHelpdesk.Contracts.Users.Views;
using RngHelpdesk.Domain.Users.Events;
using RngHelpdesk.Infrastructure.Common;

namespace RngHelpdesk.Infrastructure.Users;

public sealed class RunescapeAccountHistoryProjection :
    IProjectionHandler<RunescapeAccountLinkedEvent>,
    IProjectionHandler<RunescapeAccountRenamedEvent>,
    IProjectionHandler<RunescapeAccountDelinkedEvent>
{
    private readonly Dictionary<int, List<RunescapeAccountHistoryItem>> _history = new();

    public void Project(RunescapeAccountLinkedEvent e)
    {
        Add(e.UserId, new RunescapeAccountHistoryItem
        {
            Username = e.Username,
            IsCurrent = true,
            IsDelinked = false,
            IsPreviousName = false,
            OccurredAt = e.OccurredAt
        });
    }

    public void Project(RunescapeAccountRenamedEvent e)
    {
        // old username becomes "previous"
        Add(e.UserId, new RunescapeAccountHistoryItem
        {
            Username = e.OldUsername,
            IsCurrent = false,
            IsDelinked = false,
            IsPreviousName = true,
            OccurredAt = e.OccurredAt
        });

        // new username becomes current
        Add(e.UserId, new RunescapeAccountHistoryItem
        {
            Username = e.NewUsername,
            IsCurrent = true,
            IsDelinked = false,
            IsPreviousName = false,
            OccurredAt = e.OccurredAt
        });
    }

    public void Project(RunescapeAccountDelinkedEvent e)
    {
        Add(e.UserId, new RunescapeAccountHistoryItem
        {
            Username = e.Username,
            IsCurrent = false,
            IsDelinked = true,
            IsPreviousName = false,
            OccurredAt = e.OccurredAt
        });
    }

    private void Add(int userId, RunescapeAccountHistoryItem item)
    {
        if (!_history.TryGetValue(userId, out var list))
        {
            list = new List<RunescapeAccountHistoryItem>();
            _history[userId] = list;
        }

        list.Add(item);
    }

    public IReadOnlyList<RunescapeAccountHistoryItem> GetForUser(int userId)
        => _history.TryGetValue(userId, out var list)
            ? list
            : Array.Empty<RunescapeAccountHistoryItem>();
}
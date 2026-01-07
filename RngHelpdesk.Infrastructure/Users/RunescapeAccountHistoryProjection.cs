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
            ChangeType = RunescapeAccountChangeType.Linked,
            Username = e.Username,
            OccurredAt = e.OccurredAt
        });
    }

    public void Project(RunescapeAccountRenamedEvent e)
    {
        Add(e.UserId, new RunescapeAccountHistoryItem
        {
            ChangeType = RunescapeAccountChangeType.Renamed,
            OldUsername = e.OldUsername,
            NewUsername = e.NewUsername,
            OccurredAt = e.OccurredAt
        });
    }

    public void Project(RunescapeAccountDelinkedEvent e)
    {
        Add(e.UserId, new RunescapeAccountHistoryItem
        {
            ChangeType = RunescapeAccountChangeType.Delinked,
            Username = e.Username,
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
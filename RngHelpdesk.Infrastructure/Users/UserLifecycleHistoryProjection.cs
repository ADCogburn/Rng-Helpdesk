using RngHelpdesk.Contracts.Users.Views;
using RngHelpdesk.Domain.Users.Events;
using RngHelpdesk.Infrastructure.Common;

namespace RngHelpdesk.Infrastructure.Users;

public sealed class UserLifecycleHistoryProjection :
    IProjectionState,
    IProjectionHandler<UserDeactivatedEvent>,
    IProjectionHandler<UserReactivatedEvent>,
    IUserLifecycleHistoryReadStore
{
    private readonly Dictionary<ulong, List<UserLifecycleHistoryItem>> _history = new();
    private readonly object _lock = new();

    public bool IsEmpty
    {
        get { lock (_lock) { return _history.Count == 0; } }
    }

    public IReadOnlyList<UserLifecycleHistoryItem> GetLifecycleHistoryForUserById(ulong userId)
    {
        lock (_lock)
        {
            return _history.TryGetValue(userId, out var list)
                ? list.ToList()
                : Array.Empty<UserLifecycleHistoryItem>();
        }
    }

    #region Projections

    public void Project(UserDeactivatedEvent e)
    {
        lock (_lock)
        {
            AddLifecycleHistoryItem(e.UserId, new UserLifecycleHistoryItem
            {
                Action = "Deactivated",
                OccurredAt = e.OccurredAt
            });
        }
    }

    public void Project(UserReactivatedEvent e)
    {
        lock (_lock)
        {
            AddLifecycleHistoryItem(e.UserId, new UserLifecycleHistoryItem
            {
                Action = "Reactivated",
                OccurredAt = e.OccurredAt
            });
        }
    }

    private void AddLifecycleHistoryItem(ulong userId, UserLifecycleHistoryItem item)
    {
        if (!_history.TryGetValue(userId, out var list))
        {
            list = new List<UserLifecycleHistoryItem>();
            _history[userId] = list;
        }

        list.Add(item);
    }

    #endregion
}
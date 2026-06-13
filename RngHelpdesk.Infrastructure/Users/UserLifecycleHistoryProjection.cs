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

    public bool IsEmpty => _history.Count == 0;

    public IReadOnlyList<UserLifecycleHistoryItem> GetLifecycleHistoryForUserById(ulong userId)
        => _history.TryGetValue(userId, out var list)
            ? list
            : Array.Empty<UserLifecycleHistoryItem>();

    #region Projections

    public void Project(UserDeactivatedEvent e)
    {
        AddLifecycleHistoryItem(e.UserId, new UserLifecycleHistoryItem
        {
            Action = "Deactivated",
            OccurredAt = e.OccurredAt
        });
    }

    public void Project(UserReactivatedEvent e)
    {
        AddLifecycleHistoryItem(e.UserId, new UserLifecycleHistoryItem
        {
            Action = "Reactivated",
            OccurredAt = e.OccurredAt
        });
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
using RngHelpdesk.Contracts.Users.Views;
using RngHelpdesk.Domain.Users.Events;
using RngHelpdesk.Infrastructure.Common;

namespace RngHelpdesk.Infrastructure.Users;

public sealed class UserLifecycleHistoryProjection :
    IProjectionHandler<UserDeactivatedEvent>,
    IProjectionHandler<UserReactivatedEvent>
{
    private readonly Dictionary<int, List<UserLifecycleHistoryItem>> _history = new();

    public void Project(UserDeactivatedEvent e)
    {
        Add(e.UserId, new UserLifecycleHistoryItem
        {
            Action = "Deactivated",
            OccurredAt = e.OccurredAt
        });
    }

    public void Project(UserReactivatedEvent e)
    {
        Add(e.UserId, new UserLifecycleHistoryItem
        {
            Action = "Reactivated",
            OccurredAt = e.OccurredAt
        });
    }

    private void Add(int userId, UserLifecycleHistoryItem item)
    {
        if (!_history.TryGetValue(userId, out var list))
        {
            list = new List<UserLifecycleHistoryItem>();
            _history[userId] = list;
        }

        list.Add(item);
    }

    public IReadOnlyList<UserLifecycleHistoryItem> GetForUser(int userId)
        => _history.TryGetValue(userId, out var list)
            ? list
            : Array.Empty<UserLifecycleHistoryItem>();
}
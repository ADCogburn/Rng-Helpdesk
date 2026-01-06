using RngHelpdesk.Contracts.Points.Views;
using RngHelpdesk.Infrastructure.Common;

namespace RngHelpdesk.Infrastructure.Points;

/// <summary>
/// Projection (read-view-creation) of the point history of a user whenever points are added or removed.
/// </summary>
public sealed class PointHistoryProjection : IProjectionHandler<ClanPointsChangedEvent>
{
    private readonly Dictionary<int, List<PointHistoryItem>> _store = new();

    public void Project(ClanPointsChangedEvent e)
    {
        if (!_store.TryGetValue(e.UserId, out var history))
        {
            history = new List<PointHistoryItem>();
            _store[e.UserId] = history;
        }

        history.Add(new PointHistoryItem
        {
            Delta = e.Delta,
            Reason = e.Reason,
            OccurredAt = e.OccurredAt
        });
    }

    // Expose read access
    public IReadOnlyList<PointHistoryItem> GetForUser(int userId)
        => _store.TryGetValue(userId, out var history)
            ? history
            : Array.Empty<PointHistoryItem>();

    public int GetCountForUser(int userId)
        => _store.TryGetValue(userId, out var history)
            ? history.Count
            : 0;
}

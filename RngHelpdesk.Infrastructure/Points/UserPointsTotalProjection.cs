using RngHelpdesk.Infrastructure.Common;

namespace RngHelpdesk.Infrastructure.Points;

public sealed class UserPointsTotalProjection : IProjectionState, IProjectionHandler<ClanPointsChangedEvent>
{
    private readonly Dictionary<int, int> _totals = new();

    public bool IsEmpty => _totals.Count == 0;

    public void Project(ClanPointsChangedEvent e)
    {
        if (!_totals.ContainsKey(e.ChangedByUserId))
        {
            _totals[e.ChangedByUserId] = 0;
        }

        _totals[e.ChangedByUserId] += e.Delta;
    }

    public int GetTotalPoints(int userId)
        => _totals.TryGetValue(userId, out var total)
            ? total
            : 0;
}
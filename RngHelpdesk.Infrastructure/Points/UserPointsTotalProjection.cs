using RngHelpdesk.Infrastructure.Common;

namespace RngHelpdesk.Infrastructure.Points;

public sealed class UserPointsTotalProjection : IProjectionHandler<ClanPointsChangedEvent>
{
    private readonly Dictionary<int, int> _totals = new();

    public void Project(ClanPointsChangedEvent e)
    {
        if (!_totals.ContainsKey(e.UserId))
        {
            _totals[e.UserId] = 0;
        }

        _totals[e.UserId] += e.Delta;
    }

    public int GetTotalPoints(int userId)
        => _totals.TryGetValue(userId, out var total)
            ? total
            : 0;
}
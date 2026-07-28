using RngHelpdesk.Contracts.Security;

namespace RngHelpdesk.Contracts.Common.Ranks;

public sealed class RankResolver
{
    private readonly IReadOnlyList<RankThreshold> _thresholds;

    public RankResolver(IRankThresholdProvider provider)
    {
        // Constructor injection is synchronous, and RankResolver itself is out of this ticket's
        // scope (only IRankThresholdProvider is), so this is the one deliberate sync/async
        // boundary crossing: a one-time, startup-only block against a provider with no real I/O.
        _thresholds = provider.GetThresholdsAsync()
            .GetAwaiter()
            .GetResult()
            .OrderByDescending(t => t.PointsRequired)
            .ToList();
    }

    /// <summary>
    /// A User's rank is determined by their points, unless they are an admin of some sort (which then overrides the rank regardless of points).
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public Rank Resolve(
       AppRole appRole,
       int totalClanPoints)
    {
        if (appRole != AppRole.Member)
        {
            return RankHelper.FromAppRole(appRole);
        }

        var match = _thresholds.FirstOrDefault(t => totalClanPoints >= t.PointsRequired);

        if (match is null)
            throw new InvalidOperationException("No rank threshold matched.");

        return match.Rank;
    }
}
using RngHelpdesk.Contracts.Common.Ranks;
using RngHelpdesk.Domain.Users;

namespace RngHelpdesk.Contracts.Common.Ranks;

public sealed class RankResolver
{
    private readonly IReadOnlyList<RankThreshold> _thresholds;

    public RankResolver(IRankThresholdProvider provider)
    {
        _thresholds = provider.GetThresholds()
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
       AuthorityRole authorityRole,
       int totalClanPoints)
    {
        if (authorityRole != AuthorityRole.Member)
        {
            return RankHelper.FromAuthority(authorityRole);
        }

        var match = _thresholds.FirstOrDefault(t => totalClanPoints >= t.PointsRequired);

        if (match is null)
            throw new InvalidOperationException("No rank threshold matched.");

        return match.Rank;
    }
}
using RngHelpdesk.Contracts.Common.Ranks;

namespace RngHelpdesk.Contracts.Public;

public sealed class GetPublicOverviewResponse
{
    public int ActiveMemberCount { get; init; }
    public long TotalClanPoints { get; init; }
    public IReadOnlyList<PublicRankCountItem> RankDistribution { get; init; } = [];
}

public sealed record PublicRankCountItem(Rank Rank, int Count);

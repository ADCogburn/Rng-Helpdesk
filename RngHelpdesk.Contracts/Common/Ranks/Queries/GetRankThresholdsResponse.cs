using RngHelpdesk.Contracts.Common.Ranks.Views;

namespace RngHelpdesk.Contracts.Common.Ranks.Queries;

public sealed class GetRankThresholdsResponse
{
    public IReadOnlyList<RankThresholdView> Thresholds { get; init; } = [];
}

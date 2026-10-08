using RngHelpdesk.Contracts.Common.Ranks.Views;

namespace RngHelpdesk.Contracts.Public;

public sealed class GetPublicRanksResponse
{
    public IReadOnlyList<RankThresholdView> Ranks { get; init; } = [];
}

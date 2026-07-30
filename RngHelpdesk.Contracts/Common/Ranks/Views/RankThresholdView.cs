using RngHelpdesk.Contracts.Common.Ranks;

namespace RngHelpdesk.Contracts.Common.Ranks.Views;

public sealed record RankThresholdView(Rank Rank, int PointsRequired);

using RngHelpdesk.Contracts.Common.Ranks;

namespace RngHelpdesk.Contracts.Common.Ranks.Commands;

public sealed record UpdateRankThresholdCommand
(
    Rank Rank,
    int PointsRequired
);

using RngHelpdesk.Contracts.Common.Ranks;

namespace RngHelpdesk.Contracts.Common.Ranks.Commands;

public sealed record UpdateRankThresholdCommand
(
    ulong ActingUserId,
    Rank Rank,
    int PointsRequired
);

namespace RngHelpdesk.Contracts.Common.Ranks;

public interface IRankThresholdProvider
{
    IReadOnlyList<RankThreshold> GetThresholds();
}

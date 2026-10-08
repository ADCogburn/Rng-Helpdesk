namespace RngHelpdesk.Contracts.Common.Ranks;

public interface IRankThresholdProvider
{
    Task<IReadOnlyList<RankThreshold>> GetThresholdsAsync(CancellationToken ct = default);
}

namespace RngHelpdesk.Contracts.Common.Ranks;

/// <summary>
/// Write side for rank thresholds, kept separate from the read-only <see cref="IRankThresholdProvider"/>
/// (mirroring the projection/repository read-write split used elsewhere in this codebase).
/// </summary>
public interface IRankThresholdRepository
{
    Task UpdatePointsRequiredAsync(Rank rank, int pointsRequired, CancellationToken ct = default);
}

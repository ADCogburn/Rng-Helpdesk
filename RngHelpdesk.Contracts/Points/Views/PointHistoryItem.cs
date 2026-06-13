using RngHelpdesk.Contracts.Common.Ranks;

namespace RngHelpdesk.Contracts.Points.Views;

/// <summary>
/// Represents a single point-related event.
/// </summary>
public sealed class PointHistoryItem
{
    public int Delta { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DateTimeOffset OccurredAt { get; init; }

    public Rank? RankBefore { get; init; }
    public Rank? RankAfter { get; init; }
}

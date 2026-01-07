namespace RngHelpdesk.Contracts.Points.Views;

/// <summary>
/// Represents a single point-related event.
/// </summary>
public sealed class PointHistoryItem
{
    public int Delta { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DateTime OccurredAt { get; init; }
}

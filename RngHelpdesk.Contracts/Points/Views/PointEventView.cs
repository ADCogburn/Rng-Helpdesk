namespace RngHelpdesk.Contracts.Points.Views;

/// <summary>
/// Represents a single point-related event.
/// </summary>
public sealed class PointEventView
{
    public int UserId { get; init; }
    public int Delta { get; init; }
    public int BalanceAfterEvent { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DateTime OccurredAt { get; init; }
}

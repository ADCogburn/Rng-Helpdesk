using RngHelpdesk.Contracts.Points.Views;

namespace RngHelpdesk.Contracts.Points.Queries;

/// <summary>
/// The history of point changes over time for a user.
/// </summary>
public sealed class GetPointHistoryForUserResponse
{
    public int UserId { get; init; }

    /// <summary>
    /// Total number of events available (ignoring pagination).
    /// </summary>
    public int TotalEventCount { get; init; }

    /// <summary>
    /// Events returned for this request.
    /// </summary>
    public IReadOnlyList<PointHistoryItem> Events { get; init; } = [];
}

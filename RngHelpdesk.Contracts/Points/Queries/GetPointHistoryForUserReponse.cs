using RngHelpdesk.Contracts.Points.Views;

namespace RngHelpdesk.Contracts.Points.Queries;

/// <summary>
/// The history of point changes over time for a user.
/// TODO: is this good enough? Does it need pagination here? Is there a better way to send this back to the adapters?
/// </summary>
public sealed class GetPointHistoryForUserReponse
{
    public int EventCount { get; init; }
    public IReadOnlyCollection<PointEventView> PointEvents { get; init; } = [];
}

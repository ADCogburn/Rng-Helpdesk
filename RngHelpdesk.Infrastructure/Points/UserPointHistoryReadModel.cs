using RngHelpdesk.Contracts.Points.Views;

namespace RngHelpdesk.Infrastructure.Points;

public sealed class UserPointHistoryReadModel
{
    public IReadOnlyList<PointEventView> GetPointEventsForUser(int userId)
    {
        // Placeholder implementation - should be a SQL query for getting all of the user's point events
        return new List<PointEventView>()
        {
            new PointEventView
            {
                UserId = userId,
                Delta = 100,
                BalanceAfterEvent = 1000,
                Reason = "Initial points",
                OccurredAt = DateTime.UtcNow.AddDays(-10)
            },
        };
    }
}

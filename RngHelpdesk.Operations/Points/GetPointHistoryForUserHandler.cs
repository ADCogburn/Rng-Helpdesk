using RngHelpdesk.Contracts.Points.Queries;
using RngHelpdesk.Contracts.Points.Views;
using RngHelpdesk.Contracts.Security;

namespace RngHelpdesk.Operations.Points;

public sealed class GetPointHistoryForUserHandler
{
    public GetPointHistoryForUserReponse Handle(IRequestContext requestContext, int userId)
    {
        AuthorizationRules.RequireAdminRole(requestContext);

        // Placeholder implementation - this should request the Infra layer for the data that was preserved via Projections when the events were emitted.
        return new GetPointHistoryForUserReponse
        {
            EventCount = 1,
            PointEvents = [
                new PointEventView()
                {
                    UserId = userId,
                    Delta = 10,
                    Reason = "Won SotW",
                    OccurredAt = DateTime.UtcNow
                }],
        };
    }
}

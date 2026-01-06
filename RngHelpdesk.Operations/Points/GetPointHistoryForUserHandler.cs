using RngHelpdesk.Contracts.Points.Queries;
using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Infrastructure.Points;

namespace RngHelpdesk.Operations.Points;

public sealed class GetPointHistoryForUserHandler
{
    private readonly PointHistoryProjection _projection;

    public GetPointHistoryForUserHandler(PointHistoryProjection projection)
    {
        _projection = projection;
    }

    public GetPointHistoryForUserResponse Handle(
        IRequestContext context,
        int userId)
    {
        AuthorizationRules.RequireAdminRole(context);

        var events = _projection.GetForUser(userId);

        return new GetPointHistoryForUserResponse
        {
            UserId = userId,
            TotalEventCount = _projection.GetCountForUser(userId),
            Events = events
        };
    }
}


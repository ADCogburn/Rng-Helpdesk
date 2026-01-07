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
        GetPointHistoryForUserQuery query)
    {
        AuthorizationRules.RequireAdminRole(context);

        var events = _projection.GetForUser(query.UserId);

        return new GetPointHistoryForUserResponse
        {
            UserId = query.UserId,
            TotalEventCount = _projection.GetCountForUser(query.UserId),
            Events = events
        };
    }
}


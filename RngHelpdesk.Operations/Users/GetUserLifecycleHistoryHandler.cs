using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Infrastructure.Users;

namespace RngHelpdesk.Operations.Users;

public sealed class GetUserLifecycleHistoryHandler
{
    private readonly UserLifecycleHistoryProjection _history;

    public GetUserLifecycleHistoryHandler(UserLifecycleHistoryProjection history)
    {
        _history = history;
    }

    public GetUserLifecycleHistoryResponse Handle(
        IRequestContext requestContext,
        GetUserLifecycleHistoryQuery query)
    {
        AuthorizationRules.RequireAdminRole(requestContext);

        return new GetUserLifecycleHistoryResponse
        {
            UserId = query.UserId,
            History = _history.GetForUser(query.UserId)
        };
    }
}
using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Infrastructure.Users;

namespace RngHelpdesk.Operations.Users.RunescapeAccounts;

public sealed class GetRunescapeAccountHistoryHandler
{
    private readonly RunescapeAccountHistoryProjection _history;

    public GetRunescapeAccountHistoryHandler(
        RunescapeAccountHistoryProjection history)
    {
        _history = history;
    }

    public GetRunescapeAccountHistoryResponse Handle(
        IRequestContext requestContext,
        GetRunescapeAccountHistoryQuery query)
    {
        AuthorizationRules.RequireAuthentication(requestContext);

        return new GetRunescapeAccountHistoryResponse
        {
            UserId = query.UserId,
            History = _history.GetForUser(query.UserId)
        };
    }
}

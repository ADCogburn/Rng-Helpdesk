using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Contracts.Users.Views;
using RngHelpdesk.Infrastructure.Users;

namespace RngHelpdesk.Operations.Users.RunescapeAccounts;

public sealed class GetRunescapeAccountHandler
{
    private readonly UserSummaryProjection _users;

    public GetRunescapeAccountHandler(UserSummaryProjection users)
    {
        _users = users;
    }

    public GetRunescapeAccountsResponse Handle(
        IRequestContext requestContext,
        GetRunescapeAccountsQuery query)
    {
        AuthorizationRules.RequireAuthentication(requestContext);

        var user = _users.GetSingleById(query.UserId);

        return new GetRunescapeAccountsResponse
        {
            Accounts = user.RunescapeAccounts
                .Select(a => new RunescapeAccountView
                {
                    Username = a.Username
                })
                .ToList()
        };
    }
}
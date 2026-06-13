using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Contracts.Users.Views;
using RngHelpdesk.Infrastructure.Users;

namespace RngHelpdesk.Operations.Users.RunescapeAccounts;

public sealed class GetRunescapeAccountHandler(IUserSummaryReadStore userSummaryReadStore)
{
    public QueryResult<GetRunescapeAccountsResponse> Handle(GetRunescapeAccountsQuery query)
    {
        if (!userSummaryReadStore.TryGetById(query.UserId, out var user) || user is null)
            return QueryResult<GetRunescapeAccountsResponse>.Fail("User not found.");

        return QueryResult<GetRunescapeAccountsResponse>.Ok(new GetRunescapeAccountsResponse(
            Accounts: user.RunescapeAccounts
                .Select(a => new RunescapeAccountView(Username: a.Username))
                .ToList()
        ));
    }
}
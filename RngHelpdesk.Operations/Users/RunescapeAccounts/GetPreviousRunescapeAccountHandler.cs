using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Infrastructure.Users.RunescapeAccount;

namespace RngHelpdesk.Operations.Users.RunescapeAccounts;

public sealed class GetPreviousRunescapeAccountsHandler(IRunescapeAccountHistoryReadStore runescapeAccountHistoryReadStore)
{
    public QueryResult<GetRunescapeAccountsResponse> Handle(ulong userId)
    {
        var accounts = runescapeAccountHistoryReadStore.GetPreviousRunescapeAccounts(userId);

        return QueryResult<GetRunescapeAccountsResponse>.Ok(new GetRunescapeAccountsResponse(
            Accounts: accounts
        ));
    }
}
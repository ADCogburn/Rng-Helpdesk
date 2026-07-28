using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Infrastructure.Users.RunescapeAccount;
using RngHelpdesk.Operations.Common;

namespace RngHelpdesk.Operations.Users.RunescapeAccounts;

public sealed class GetPreviousRunescapeAccountsHandler(IRunescapeAccountHistoryReadStore runescapeAccountHistoryReadStore) : IQueryHandler<GetPreviousRunescapeAccountsQuery, GetRunescapeAccountsResponse>
{
    public Task<QueryResult<GetRunescapeAccountsResponse>> Handle(GetPreviousRunescapeAccountsQuery query, CancellationToken cancellationToken = default)
    {
        var accounts = runescapeAccountHistoryReadStore.GetPreviousRunescapeAccounts(query.UserId);

        return Task.FromResult(QueryResult<GetRunescapeAccountsResponse>.Ok(new GetRunescapeAccountsResponse(
            Accounts: accounts
        )));
    }
}
using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Infrastructure.Users.RunescapeAccount;

namespace RngHelpdesk.Operations.Users.RunescapeAccounts;

public sealed class GetRunescapeAccountHistoryHandler(IRunescapeAccountHistoryReadStore runescapeAccountHistoryReadStore)
{

    public QueryResult<GetRunescapeAccountHistoryResponse> Handle(ulong userId)
    {
        var historicalRsnChangedEvents = runescapeAccountHistoryReadStore.GetHistory(userId);

        return QueryResult<GetRunescapeAccountHistoryResponse>.Ok(new GetRunescapeAccountHistoryResponse(historicalRsnChangedEvents));
    }
}

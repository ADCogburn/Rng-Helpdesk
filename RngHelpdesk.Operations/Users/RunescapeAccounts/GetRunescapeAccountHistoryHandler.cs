using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Infrastructure.Users.RunescapeAccount;
using RngHelpdesk.Operations.Common;

namespace RngHelpdesk.Operations.Users.RunescapeAccounts;

public sealed class GetRunescapeAccountHistoryHandler(IRunescapeAccountHistoryReadStore runescapeAccountHistoryReadStore) : IQueryHandler<GetRunescapeAccountHistoryQuery, GetRunescapeAccountHistoryResponse>
{

    public async Task<QueryResult<GetRunescapeAccountHistoryResponse>> Handle(GetRunescapeAccountHistoryQuery query, CancellationToken cancellationToken = default)
    {
        var historicalRsnChangedEvents = await runescapeAccountHistoryReadStore.GetHistoryAsync(query.UserId, cancellationToken);

        return QueryResult<GetRunescapeAccountHistoryResponse>.Ok(new GetRunescapeAccountHistoryResponse(historicalRsnChangedEvents));
    }
}

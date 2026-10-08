using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Infrastructure.Users;
using RngHelpdesk.Operations.Common;

namespace RngHelpdesk.Operations.Users;

public sealed class GetUserLifecycleHistoryHandler(
    IUserLifecycleHistoryReadStore userLifecycleHistoryReadStore,
    IUserSummaryReadStore userSummaryReadStore) : IQueryHandler<GetUserLifecycleHistoryQuery, GetUserLifecycleHistoryResponse>
{
    public async Task<QueryResult<GetUserLifecycleHistoryResponse>> Handle(GetUserLifecycleHistoryQuery query, CancellationToken cancellationToken = default)
    {
        var user = await userSummaryReadStore.GetByIdAsync(query.UserId, cancellationToken);

        if (user is null)
            return QueryResult<GetUserLifecycleHistoryResponse>.Fail("User not found.");

        return QueryResult<GetUserLifecycleHistoryResponse>.Ok(
            new GetUserLifecycleHistoryResponse
            {
                UserId = query.UserId,
                History = await userLifecycleHistoryReadStore.GetLifecycleHistoryForUserByIdAsync(query.UserId, cancellationToken)
            });
    }
}
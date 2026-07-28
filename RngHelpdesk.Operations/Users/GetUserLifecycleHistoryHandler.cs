using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Infrastructure.Users;
using RngHelpdesk.Operations.Common;

namespace RngHelpdesk.Operations.Users;

public sealed class GetUserLifecycleHistoryHandler(
    IUserLifecycleHistoryReadStore userLifecycleHistoryReadStore,
    IUserSummaryReadStore userSummaryReadStore) : IQueryHandler<GetUserLifecycleHistoryQuery, GetUserLifecycleHistoryResponse>
{
    public Task<QueryResult<GetUserLifecycleHistoryResponse>> Handle(GetUserLifecycleHistoryQuery query, CancellationToken cancellationToken = default)
    {
        if (!userSummaryReadStore.TryGetById(query.UserId, out _))
            return Task.FromResult(QueryResult<GetUserLifecycleHistoryResponse>.Fail("User not found."));

        return Task.FromResult(QueryResult<GetUserLifecycleHistoryResponse>.Ok(
            new GetUserLifecycleHistoryResponse
            {
                UserId = query.UserId,
                History = userLifecycleHistoryReadStore.GetLifecycleHistoryForUserById(query.UserId)
            }));
    }
}
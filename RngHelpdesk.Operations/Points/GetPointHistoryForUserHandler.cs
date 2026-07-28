using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Points.Queries;
using RngHelpdesk.Infrastructure.Points;
using RngHelpdesk.Infrastructure.Users;
using RngHelpdesk.Operations.Common;

namespace RngHelpdesk.Operations.Points;

public sealed class GetPointHistoryForUserHandler(
    IPointHistoryReadStore pointHistoryReadStore,
    IUserSummaryReadStore userSummaryReadStore) : IQueryHandler<GetPointHistoryForUserQuery, GetPointHistoryForUserResponse>
{
    public Task<QueryResult<GetPointHistoryForUserResponse>> Handle(GetPointHistoryForUserQuery query, CancellationToken cancellationToken = default)
    {
        if (!userSummaryReadStore.TryGetById(query.UserId, out _))
            return Task.FromResult(QueryResult<GetPointHistoryForUserResponse>.Fail("User not found"));

        var events = pointHistoryReadStore.GetPointHistoryForUser(query.UserId);
        var count = pointHistoryReadStore.GetCountForUser(query.UserId);

        if (count == 0)
            throw new InvalidOperationException("Point history projection is missing expected user creation event.");

        return Task.FromResult(QueryResult<GetPointHistoryForUserResponse>.Ok(
            new GetPointHistoryForUserResponse
            {
                UserId = query.UserId,
                TotalEventCount = count,
                Events = events
            }));
    }
}


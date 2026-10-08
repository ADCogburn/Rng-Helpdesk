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
    public async Task<QueryResult<GetPointHistoryForUserResponse>> Handle(GetPointHistoryForUserQuery query, CancellationToken cancellationToken = default)
    {
        var user = await userSummaryReadStore.GetByIdAsync(query.UserId, cancellationToken);

        if (user is null)
            return QueryResult<GetPointHistoryForUserResponse>.Fail("User not found");

        var events = await pointHistoryReadStore.GetPointHistoryForUserAsync(query.UserId, cancellationToken);
        var count = await pointHistoryReadStore.GetCountForUserAsync(query.UserId, cancellationToken);

        if (count == 0)
            throw new InvalidOperationException("Point history projection is missing expected user creation event.");

        return QueryResult<GetPointHistoryForUserResponse>.Ok(
            new GetPointHistoryForUserResponse
            {
                UserId = query.UserId,
                TotalEventCount = count,
                Events = events
            });
    }
}


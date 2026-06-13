using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Points.Queries;
using RngHelpdesk.Infrastructure.Points;
using RngHelpdesk.Infrastructure.Users;

namespace RngHelpdesk.Operations.Points;

public sealed class GetPointHistoryForUserHandler(
    IPointHistoryReadStore pointHistoryReadStore,
    IUserSummaryReadStore userSummaryReadStore)
{
    public CommandResult<GetPointHistoryForUserResponse> Handle(GetPointHistoryForUserQuery query)
    {
        if (!userSummaryReadStore.TryGetById(query.UserId, out _))
            return CommandResult<GetPointHistoryForUserResponse>.Fail("User not found");

        var events = pointHistoryReadStore.GetPointHistoryForUser(query.UserId);
        var count = pointHistoryReadStore.GetCountForUser(query.UserId);

        if (count == 0)
            throw new InvalidOperationException("Point history projection is missing expected user creation event.");

        return CommandResult<GetPointHistoryForUserResponse>.Ok(
            new GetPointHistoryForUserResponse
            {
                UserId = query.UserId,
                TotalEventCount = count,
                Events = events
            });
    }
}


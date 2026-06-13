using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Infrastructure.Users;

namespace RngHelpdesk.Operations.Users;

public sealed class GetUserLifecycleHistoryHandler(
    IUserLifecycleHistoryReadStore userLifecycleHistoryReadStore,
    IUserSummaryReadStore userSummaryReadStore)
{
    public CommandResult<GetUserLifecycleHistoryResponse> Handle(GetUserLifecycleHistoryQuery query)
    {
        if (!userSummaryReadStore.TryGetById(query.UserId, out _))
            return CommandResult<GetUserLifecycleHistoryResponse>.Fail("User not found.");

        return CommandResult<GetUserLifecycleHistoryResponse>.Ok(
            new GetUserLifecycleHistoryResponse
            {
                UserId = query.UserId,
                History = userLifecycleHistoryReadStore.GetLifecycleHistoryForUserById(query.UserId)
            });
    }
}
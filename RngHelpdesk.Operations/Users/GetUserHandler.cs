using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Infrastructure.Users;
using RngHelpdesk.Operations.Helpers;

namespace RngHelpdesk.Operations.Users;

public sealed class GetUserHandler(IUserSummaryReadStore userSummaryReadStore)
{
    public QueryResult<GetUserResponse> Handle(GetUserByIdQuery query)
    {
        if (!userSummaryReadStore.TryGetById(query.UserId, out var user) || user is null)
            return QueryResult<GetUserResponse>.Fail("User not found.");

        return QueryResult<GetUserResponse>.Ok(new GetUserResponse(user.ToDto()));
    }

    public QueryResult<GetUserResponse> Handle(GetUserByRunescapeUsernameQuery query)
    {
        if (string.IsNullOrWhiteSpace(query.RunescapeUsername))
            return QueryResult<GetUserResponse>.Fail("Blank username was requested.");

        if (!userSummaryReadStore.TryGetByRunescapeUsername(query.RunescapeUsername, out var user) || user is null)
            return QueryResult<GetUserResponse>.Fail("User not found.");

        return QueryResult<GetUserResponse>.Ok(new GetUserResponse(user.ToDto()));
    }
}
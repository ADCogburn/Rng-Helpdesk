using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Infrastructure.Users;
using RngHelpdesk.Operations.Common;

namespace RngHelpdesk.Operations.Users;

public sealed class GetUserByRunescapeUsernameHandler(IUserSummaryReadStore userSummaryReadStore) : IQueryHandler<GetUserByRunescapeUsernameQuery, GetUserResponse>
{
    public async Task<QueryResult<GetUserResponse>> Handle(GetUserByRunescapeUsernameQuery query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query.RunescapeUsername))
            return QueryResult<GetUserResponse>.Fail("Blank username was requested.");

        var user = await userSummaryReadStore.GetByRunescapeUsernameAsync(query.RunescapeUsername, cancellationToken);

        if (user is null)
            return QueryResult<GetUserResponse>.Fail("User not found.");

        return QueryResult<GetUserResponse>.Ok(GetUserResponseMapper.MapToResponse(user));
    }
}

using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Infrastructure.Users;
using RngHelpdesk.Operations.Common;

namespace RngHelpdesk.Operations.Users;

public sealed class GetUserByRunescapeUsernameHandler(IUserSummaryReadStore userSummaryReadStore) : IQueryHandler<GetUserByRunescapeUsernameQuery, GetUserResponse>
{
    public Task<QueryResult<GetUserResponse>> Handle(GetUserByRunescapeUsernameQuery query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query.RunescapeUsername))
            return Task.FromResult(QueryResult<GetUserResponse>.Fail("Blank username was requested."));

        if (!userSummaryReadStore.TryGetByRunescapeUsername(query.RunescapeUsername, out var user) || user is null)
            return Task.FromResult(QueryResult<GetUserResponse>.Fail("User not found."));

        return Task.FromResult(QueryResult<GetUserResponse>.Ok(GetUserResponseMapper.MapToResponse(user)));
    }
}

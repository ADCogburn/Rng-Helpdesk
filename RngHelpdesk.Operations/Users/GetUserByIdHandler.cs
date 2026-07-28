using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Infrastructure.Users;
using RngHelpdesk.Operations.Common;

namespace RngHelpdesk.Operations.Users;

public sealed class GetUserByIdHandler(IUserSummaryReadStore userSummaryReadStore) : IQueryHandler<GetUserByIdQuery, GetUserResponse>
{
    public Task<QueryResult<GetUserResponse>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken = default)
    {
        if (!userSummaryReadStore.TryGetById(query.UserId, out var user) || user is null)
            return Task.FromResult(QueryResult<GetUserResponse>.Fail("User not found."));

        return Task.FromResult(QueryResult<GetUserResponse>.Ok(GetUserResponseMapper.MapToResponse(user)));
    }
}

using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Infrastructure.Users;
using RngHelpdesk.Operations.Common;

namespace RngHelpdesk.Operations.Users;

public sealed class GetUserByIdHandler(IUserSummaryReadStore userSummaryReadStore) : IQueryHandler<GetUserByIdQuery, GetUserResponse>
{
    public async Task<QueryResult<GetUserResponse>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken = default)
    {
        var user = await userSummaryReadStore.GetByIdAsync(query.UserId, cancellationToken);

        if (user is null)
            return QueryResult<GetUserResponse>.Fail("User not found.");

        return QueryResult<GetUserResponse>.Ok(GetUserResponseMapper.MapToResponse(user));
    }
}

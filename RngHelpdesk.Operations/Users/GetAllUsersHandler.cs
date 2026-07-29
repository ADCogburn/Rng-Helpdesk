using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Infrastructure.Users;
using RngHelpdesk.Operations.Common;

namespace RngHelpdesk.Operations.Users;

public sealed class GetAllUsersHandler(IUserSummaryReadStore userSummaryReadStore) : IQueryHandler<GetAllUsersQuery, GetAllUsersResponse>
{
    public async Task<QueryResult<GetAllUsersResponse>> Handle(GetAllUsersQuery query, CancellationToken cancellationToken = default)
    {
        var allUsers = await userSummaryReadStore.GetAllAsync(cancellationToken);

        var users = allUsers
            .Select(GetUserResponseMapper.MapToResponse)
            .ToList();

        return QueryResult<GetAllUsersResponse>.Ok(new GetAllUsersResponse
        {
            TotalCount = users.Count,
            Users = users
        });
    }
}
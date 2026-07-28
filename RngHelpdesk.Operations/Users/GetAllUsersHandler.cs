using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Infrastructure.Users;
using RngHelpdesk.Operations.Common;

namespace RngHelpdesk.Operations.Users;

public sealed class GetAllUsersHandler(IUserSummaryReadStore userSummaryReadStore) : IQueryHandler<GetAllUsersQuery, GetAllUsersResponse>
{
    public Task<QueryResult<GetAllUsersResponse>> Handle(GetAllUsersQuery query, CancellationToken cancellationToken = default)
    {
        var users = userSummaryReadStore.GetAll()
            .Select(GetUserResponseMapper.MapToResponse)
            .ToList();

        return Task.FromResult(QueryResult<GetAllUsersResponse>.Ok(new GetAllUsersResponse
        {
            TotalCount = users.Count,
            Users = users
        }));
    }
}
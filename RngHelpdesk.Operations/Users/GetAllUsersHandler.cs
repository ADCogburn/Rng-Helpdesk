using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Infrastructure.Users;

namespace RngHelpdesk.Operations.Users;

public sealed class GetAllUsersHandler(IUserSummaryReadStore userSummaryReadStore)
{
    public QueryResult<GetAllUsersResponse> Handle()
    {
        var users = userSummaryReadStore.GetAll()
            .Select(u => new GetUserResponse
            {
                Id = u.UserId,
                AppRole = u.AppRole,
                ClanPoints = u.ClanPoints,
                Rank = u.Rank,
                IsActive = u.IsActive,
                DateCreated = u.DateCreated,
                RunescapeAccounts = u.RunescapeAccounts.ToList(),
                DiscordAccount = u.DiscordAccount
            })
            .ToList();

        return QueryResult<GetAllUsersResponse>.Ok(new GetAllUsersResponse
        {
            TotalCount = users.Count,
            Users = users
        });
    }
}
using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Models.Users;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Infrastructure.Users;

namespace RngHelpdesk.Operations.Users;

public sealed class GetAllUsersHandler(IUserSummaryReadStore userSummaryReadStore)
{
    public QueryResult<GetAllUsersResponse> Handle()
    {
        var users = userSummaryReadStore.GetAll()
            .Select(u => new UserDto(
                UserId: u.UserId,
                AppRole: u.AppRole,
                ClanPoints: u.ClanPoints,
                Rank: u.Rank,
                IsActive: u.IsActive,
                DateCreated: u.DateCreated,
                DiscordAccount: u.DiscordAccount,
                RunescapeAccounts: u.RunescapeAccounts.ToList()))
            .ToList();

        return QueryResult<GetAllUsersResponse>.Ok(new GetAllUsersResponse
        {
            TotalCount = users.Count,
            Users = users
        });
    }
}
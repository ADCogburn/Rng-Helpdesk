using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Contracts.Users.Views;
using RngHelpdesk.Infrastructure.Users;
using RngHelpdesk.Operations.Common;

namespace RngHelpdesk.Operations.Users.RunescapeAccounts;

public sealed class GetRunescapeAccountHandler(IUserSummaryReadStore userSummaryReadStore) : IQueryHandler<GetRunescapeAccountsQuery, GetRunescapeAccountsResponse>
{
    public async Task<QueryResult<GetRunescapeAccountsResponse>> Handle(GetRunescapeAccountsQuery query, CancellationToken cancellationToken = default)
    {
        var user = await userSummaryReadStore.GetByIdAsync(query.UserId, cancellationToken);

        if (user is null)
            return QueryResult<GetRunescapeAccountsResponse>.Fail("User not found.");

        return QueryResult<GetRunescapeAccountsResponse>.Ok(new GetRunescapeAccountsResponse(
            Accounts: user.RunescapeAccounts
                .Select(a => new RunescapeAccountView(Username: a.Username))
                .ToList()
        ));
    }
}
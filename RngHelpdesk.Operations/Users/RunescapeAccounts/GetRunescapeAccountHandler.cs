using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Contracts.Users.Views;
using RngHelpdesk.Infrastructure.Users;
using RngHelpdesk.Operations.Common;

namespace RngHelpdesk.Operations.Users.RunescapeAccounts;

public sealed class GetRunescapeAccountHandler(IUserSummaryReadStore userSummaryReadStore) : IQueryHandler<GetRunescapeAccountsQuery, GetRunescapeAccountsResponse>
{
    public Task<QueryResult<GetRunescapeAccountsResponse>> Handle(GetRunescapeAccountsQuery query, CancellationToken cancellationToken = default)
    {
        if (!userSummaryReadStore.TryGetById(query.UserId, out var user) || user is null)
            return Task.FromResult(QueryResult<GetRunescapeAccountsResponse>.Fail("User not found."));

        return Task.FromResult(QueryResult<GetRunescapeAccountsResponse>.Ok(new GetRunescapeAccountsResponse(
            Accounts: user.RunescapeAccounts
                .Select(a => new RunescapeAccountView(Username: a.Username))
                .ToList()
        )));
    }
}
using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Contracts.Users.Views;
using RngHelpdesk.Domain.Users;

namespace RngHelpdesk.Operations.Users;

public class GetRunescapeAccountHandler
{
    public GetRunescapeAccountsResponse Handle(
        IRequestContext requestContext,
        int userId)
    {
        AuthorizationRules.RequireAuthentication(requestContext);

        // TEMP: fake data

        var exampledDisabledAccount = new RunescapeAccount("Disabled_Account");
        exampledDisabledAccount.Deactivate();

        var user = new User(
            id: userId,
            role: AuthorityRole.Member,
            discordAccounts: new[] {
                new DiscordAccount(123456789012345678, "FakeDiscordId"),
                new DiscordAccount(123465789012345679, "OtherFakeId", false
            )},
            runescapeAccounts: new[]
            {
                new RunescapeAccount("FakeRSN_One"),
                new RunescapeAccount("FakeRSN_Two"),
                exampledDisabledAccount
            });

        // TEMP: get from db

        return new GetRunescapeAccountsResponse
        {
            Accounts = user.RunescapeAccounts
                .Select(a => new RunescapeAccountView
                {
                    Username = a.Username,
                    IsActive = a.IsActive
                })
                .ToList()
        };
    }
}

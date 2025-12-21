using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Domain.Users;

namespace RngHelpdesk.UseCases.Handlers.Users;

public class GetRunescapeAccountHandler
{
    public GetRunescapeAccountsResponse Handle(int id)
    {
        // TEMP: fake data

        const ulong fakeDiscordId = 123456789012345678;

        var user = new User(
            id: id,
            role: AuthorityRole.Member,
            discordAccounts: new[] { new DiscordAccount(fakeDiscordId) },
            runescapeAccounts: new[]
            {
                new RunescapeAccount("FakeRSN_One"),
                new RunescapeAccount("FakeRSN_Two")
            });

        // TEMP: get from db

        return new GetRunescapeAccountsResponse
        {
            Accounts = user.RunescapeAccounts
                .Select(a => a.Username)
                .ToList()
        };
    }
}

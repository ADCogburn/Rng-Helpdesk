using RngHelpdesk.Domain.Users;

namespace RngHelpdesk.Operations.Handlers.Users;

public sealed class LinkDiscordAccountHandler
{
    public void Handle(int userId, ulong discordId)
    {
        var user = new User(
            id: userId,
            role: AuthorityRole.Member,
            discordAccounts: [],
            runescapeAccounts: []);

        // TODO: do I pass this in, or hit discord api for this or what?
        var fakeusername = "FakeUser";

        var account = new DiscordAccount(discordId, fakeusername);

        user.DiscordAccounts.Add(account);

        // Later:
        // _userRepository.Save(user);
    }
}

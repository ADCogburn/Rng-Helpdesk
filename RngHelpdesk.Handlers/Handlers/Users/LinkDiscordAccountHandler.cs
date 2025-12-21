using RngHelpdesk.Domain.Users;

namespace RngHelpdesk.UseCases.Handlers.Users;

public sealed class LinkDiscordAccountHandler
{
    public void Handle(int userId, ulong discordId)
    {
        var user = new User(
            id: userId,
            role: AuthorityRole.Member,
            discordAccounts: [],
            runescapeAccounts: []);

        var account = new DiscordAccount(discordId);

        user.DiscordAccounts.Add(account);

        // Later:
        // _userRepository.Save(user);
    }
}

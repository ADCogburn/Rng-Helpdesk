using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Domain.Users;

namespace RngHelpdesk.Operations.Users;

public sealed class LinkDiscordAccountHandler
{
    public void Handle(
        IRequestContext requestContext,
        LinkDiscordAccountRequest request)
    {
        AuthorizationRules.RequireRole(requestContext, AuthorityRole.Owner);

        var user = new User(
            id: request.UserId,
            role: AuthorityRole.Member,
            discordAccounts: [],
            runescapeAccounts: []);

        // TODO: do I pass this in, or hit discord api for this or what?
        var fakeusername = "FakeUser";

        user.AddDiscordAccount(request.DiscordId, fakeusername);

        // Later:
        // _userRepository.Save(user);
    }
}

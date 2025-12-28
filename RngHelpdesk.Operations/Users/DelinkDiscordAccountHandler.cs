using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Contracts.Users.Commands;
using RngHelpdesk.Domain.Users;

namespace RngHelpdesk.Operations.Users;

public sealed class DelinkDiscordAccountHandler
{
    public void Handle(
        IRequestContext requestContext,
        DelinkDiscordAccountRequest request)
    {
        AuthorizationRules.RequireAdminRole(requestContext);

        var user = new User(
            id: request.UserId,
            role: AuthorityRole.Member,
            discordAccounts: [],
            runescapeAccounts: []);

        user.RemoveDiscordAccount(request.DiscordId);

        // Later:
        // _userRepository.Save(user);
    }
}

using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Contracts.Users.Commands;
using RngHelpdesk.Domain.Users;

namespace RngHelpdesk.Operations.Users;

public sealed class DelinkRunescapeAccountHandler
{
    public void Handle(
        IRequestContext requestContext,
        DelinkRunescapeAccountRequest request)
    {
        AuthorizationRules.RequireAdminRole(requestContext);

        var user = new User(
            id: request.UserId,
            role: AuthorityRole.Member,
            discordAccounts: [],
            runescapeAccounts: []);

        // TODO: do I pass this in, or hit discord api for this or what?

        user.RemoveRunescapeAccount(request.Username);

        // Later:
        // _userRepository.Save(user);
    }
}

using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Domain.Users;

namespace RngHelpdesk.Operations.Users;

public sealed class LinkRunescapeAccountHandler
{
    public void Handle(
        IRequestContext requestContext,
        LinkRunescapeAccountRequest request)
    {
        AuthorizationRules.RequireRole(requestContext, AuthorityRole.Administrator);

        // TEMP fake user (later: repository)
        var user = new User(
            id: request.UserId,
            role: AuthorityRole.Member,
            discordAccounts: [],
            runescapeAccounts: []);

        user.AddRunescapeAccount(request.Username);

        // Later:
        // _userRepository.Save(user);
    }
}
using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Domain.Users;

namespace RngHelpdesk.Operations.Handlers.Users;

public sealed class LinkRunescapeAccountHandler
{
    public void Handle(int userId, string username, IRequestContext requestContext)
    {
        AuthorizationRules.RequireMember(requestContext);
        AuthorizationRules.RequireRole(requestContext, SystemRoles.Admin);

        // TEMP fake user (later: repository)
        var user = new User(
            id: userId,
            role: AuthorityRole.Member,
            discordAccounts: [],
            runescapeAccounts: []);

        // Domain behavior (eventually this becomes a method)
        var account = new RunescapeAccount(username);

        user.RunescapeAccounts.Add(account);

        // Later:
        // _userRepository.Save(user);
    }
}
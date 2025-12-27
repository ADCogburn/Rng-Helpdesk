using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Contracts.Users.Commands;
using RngHelpdesk.Domain.Users;

namespace RngHelpdesk.Operations.Users;

public sealed class ChangeAdminStatusHandler
{
    public void Handle(
        IRequestContext context,
        ChangeAdminStatusRequest request)
    {
        AuthorizationRules.RequireSuperAdminRole(context);

        // TEMP : Get User from DB
        var user = new User(
            id: request.UserId,
            role: AuthorityRole.Member,
            discordAccounts: new[] {
                new DiscordAccount(123456789012345678, "FakeDiscordId"),
                new DiscordAccount(123465789012345679, "OtherFakeId", false
            )},
            runescapeAccounts: new[]
            {
                new RunescapeAccount("FakeRSN_One"),
                new RunescapeAccount("FakeRSN_Two")
            });

        user.ChangeAuthorityRole(request.NewRole);

        // TEMP : Save User to DB
    }
}
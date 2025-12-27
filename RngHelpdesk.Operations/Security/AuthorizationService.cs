using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Domain.Users;

namespace RngHelpdesk.Operations.Security;

public sealed class AuthorizationService
{
    public AuthorizationService()
    {
        // inject the user repo when infra is made
        // inject the actor repo when infra is made
    }

    public AuthorityRole GetAuthorityRoleForActor(Guid actorId)
    {
        // TEMP: get actor from db, finding the user from there.
        var actor = new Actor(
            id: Guid.NewGuid(),
            userId: 123,
            actorType: ActorType.WebUser);

        // TEMP: get user from db
        var user = new User(
            id: actor.UserId,
            role: AuthorityRole.Administrator,
            discordAccounts: new[] {
                new DiscordAccount(123456789012345678, "FakeDiscordId"),
                new DiscordAccount(123465789012345679, "OtherFakeId", false
            )},
            runescapeAccounts: new[]
            {
                new RunescapeAccount("FakeRSN_One"),
                new RunescapeAccount("FakeRSN_Two")
            });

        return user.AuthorityRole;
    }
}

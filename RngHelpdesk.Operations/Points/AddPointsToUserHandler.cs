using RngHelpdesk.Contracts.Points.Commands;
using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Domain.Users;

namespace RngHelpdesk.Operations.Points;

public sealed class AddPointsToUserHandler
{
    public void Handle(
        IRequestContext context,
        AddPointsToUserRequest request)
    {
        AuthorizationRules.RequireRole(context, AuthorityRole.Administrator);

        // get user from db
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

        user.AddClanPoints(request.Points, request.Reason);

        // Repo - will handle both the user and the event changes.
        // await _db.SaveChangesAsync(); 
    }
}

using Microsoft.AspNetCore.Mvc;
using RngHelpdesk.Contracts.Users;
using RngHelpdesk.Domain.Ranks;
using RngHelpdesk.Domain.Users;

namespace RngHelpdesk.Api.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    [HttpGet("{id:int}")]
    public ActionResult<UserResponse> GetById(int id)
    {
        // TEMP: fake domain object
        const ulong fakeDiscordId = 123456789012345678;
        var fakeRunescapeAccounts = new[]
        {
            new RunescapeAccount("FakeRSN_One"),
            new RunescapeAccount("FakeRSN_Two")
        };

        var user = new User(
            id: id,
            role: AuthorityRole.Member,
            discordAccounts: new[] { new DiscordAccount(fakeDiscordId) },
            runescapeAccounts: fakeRunescapeAccounts
        );

        // TEMP: inline rank resolver (refactor to pull from config in DB later)
        var thresholds = new[]
        {
            new RankThreshold(Rank.Bronze, 0),
            new RankThreshold(Rank.Iron, 10),
            new RankThreshold(Rank.Steel, 50),
            new RankThreshold(Rank.Mithril, 100),
            new RankThreshold(Rank.Adamant, 175),
            new RankThreshold(Rank.Rune, 265),
            new RankThreshold(Rank.Dragon, 375),
        };

        var rankResolver = new RankResolver(thresholds);
        var rank = rankResolver.Resolve(user);

        // Map Domain → Contract
        return Ok(new UserResponse
        {
            Id = user.Id,
            ClanPoints = user.ClanPoints,
            Rank = rank.ToString(),
            IsActive = user.IsActive
        });
    }
}

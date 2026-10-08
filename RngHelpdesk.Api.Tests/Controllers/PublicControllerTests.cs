using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RngHelpdesk.Api.Controllers;
using RngHelpdesk.Contracts.Public;
using RngHelpdesk.Domain.Users;

namespace RngHelpdesk.Api.Tests.Controllers;

public class PublicControllerTests
{
    private readonly ApiTestFixture _fixture = new();

    private PublicController CreateController()
        => new(
            _fixture.CreateGetPublicOverviewHandler(),
            _fixture.CreateGetPublicRanksHandler(),
            _fixture.CreateGetPublicLeaderboardHandler());

    [Fact]
    public void Controller_IsAnonymousAndRoutedUnderPublic()
    {
        Assert.NotNull(Attribute.GetCustomAttribute(typeof(PublicController), typeof(AllowAnonymousAttribute)));
        Assert.Equal("public", ((RouteAttribute)Attribute.GetCustomAttribute(typeof(PublicController), typeof(RouteAttribute))!).Template);
    }

    [Fact]
    public async Task GetOverview_ReturnsOk()
    {
        var result = await CreateController().GetOverview(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<GetPublicOverviewResponse>(ok.Value);
        Assert.Equal(14, response.RankDistribution.Count);
    }

    [Fact]
    public async Task GetRanks_ReturnsOk()
    {
        var result = await CreateController().GetRanks(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(14, Assert.IsType<GetPublicRanksResponse>(ok.Value).Ranks.Count);
    }

    [Fact]
    public async Task GetLeaderboard_ReturnsOkAndExposesNoIdsOrDiscordData()
    {
        await _fixture.CreateAndDispatchUserAsync(
            TestUsers.DefaultActingUserId,
            TestUsers.ValidDiscordAccount(123456789012345678, "secretDiscordName"),
            [new RunescapeAccount("Zezima")]);

        var result = await CreateController().GetLeaderboard(25, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<GetPublicLeaderboardResponse>(ok.Value);
        var entry = Assert.Single(response.Entries);
        Assert.Equal("Zezima", entry.RunescapeUsername);

        var json = JsonSerializer.Serialize(response);
        Assert.DoesNotContain("123456789012345678", json);
        Assert.DoesNotContain("secretDiscordName", json);
        Assert.DoesNotContain("discord", json, StringComparison.OrdinalIgnoreCase);
    }
}

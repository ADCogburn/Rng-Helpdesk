using Microsoft.AspNetCore.Mvc;
using RngHelpdesk.Api.Controllers;
using RngHelpdesk.Api.DTOs;
using RngHelpdesk.Contracts.Common.Ranks;
using RngHelpdesk.Contracts.Common.Ranks.Queries;

namespace RngHelpdesk.Api.Tests.Controllers;

public class RankThresholdsControllerTests
{
    private readonly ApiTestFixture _fixture = new();

    private RankThresholdsController CreateController()
    {
        var controller = new RankThresholdsController(
            _fixture.CreateGetRankThresholdsHandler(),
            _fixture.CreateUpdateRankThresholdHandler());

        ControllerTestHelpers.SetActingUser(controller, TestUsers.DefaultActingUserId);
        return controller;
    }

    [Fact]
    public async Task GetRankThresholds_ReturnsOkWithEveryConfiguredThreshold()
    {
        var controller = CreateController();

        var result = await controller.GetRankThresholds(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<GetRankThresholdsResponse>(ok.Value);
        Assert.Equal(14, response.Thresholds.Count);
    }

    [Fact]
    public async Task UpdateRankThreshold_ValidUpdate_ReturnsNoContentAndPersists()
    {
        var controller = CreateController();

        var result = await controller.UpdateRankThreshold(Rank.Steel, new UpdateRankThresholdDto { PointsRequired = 60 }, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);

        var thresholds = await _fixture.RankThresholdProvider.GetThresholdsAsync();
        Assert.Equal(60, thresholds.Single(t => t.Rank == Rank.Steel).PointsRequired);
    }

    [Fact]
    public async Task UpdateRankThreshold_NonMonotonicValue_ReturnsBadRequestWithHandlerError()
    {
        var controller = CreateController();

        // Steel's next neighbor, Mithril, requires 100 points.
        var result = await controller.UpdateRankThreshold(Rank.Steel, new UpdateRankThresholdDto { PointsRequired = 100 }, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("must be less than Mithril's threshold", (string)badRequest.Value!);
    }

    [Fact]
    public async Task UpdateRankThreshold_RankNotConfigurable_ReturnsBadRequestWithHandlerError()
    {
        var controller = CreateController();

        var result = await controller.UpdateRankThreshold(Rank.Owner, new UpdateRankThresholdDto { PointsRequired = 100 }, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("has no configurable point threshold", (string)badRequest.Value!);
    }
}

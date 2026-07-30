using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Common.Ranks;
using RngHelpdesk.Contracts.Common.Ranks.Queries;
using RngHelpdesk.Operations.Admin;

namespace RngHelpdesk.Operations.Tests.Admin;

public class GetRankThresholdsHandlerTests
{
    private readonly OperationsTestFixture _fixture = new();

    private GetRankThresholdsHandler CreateHandler()
        => new(_fixture.RankThresholdProvider);

    [Fact]
    public async Task Handle_ReturnsEveryConfiguredThreshold()
    {
        var handler = CreateHandler();

        var result = await handler.Handle(new GetRankThresholdsQuery());

        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.Equal(14, result.Value!.Thresholds.Count);
        Assert.Contains(result.Value.Thresholds, t => t.Rank == Rank.Bronze && t.PointsRequired == 0);
        Assert.Contains(result.Value.Thresholds, t => t.Rank == Rank.Zenyte && t.PointsRequired == 5000);
    }

    [Fact]
    public async Task Handle_ReflectsAPreviousUpdate()
    {
        await _fixture.RankThresholdProvider.UpdatePointsRequiredAsync(Rank.Iron, 25);

        var handler = CreateHandler();

        var result = await handler.Handle(new GetRankThresholdsQuery());

        Assert.Equal(25, result.Value!.Thresholds.Single(t => t.Rank == Rank.Iron).PointsRequired);
    }
}

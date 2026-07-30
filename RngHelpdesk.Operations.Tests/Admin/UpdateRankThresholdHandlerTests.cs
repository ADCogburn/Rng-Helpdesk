using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Common.Ranks;
using RngHelpdesk.Contracts.Common.Ranks.Commands;
using RngHelpdesk.Operations.Admin;

namespace RngHelpdesk.Operations.Tests.Admin;

public class UpdateRankThresholdHandlerTests
{
    private readonly OperationsTestFixture _fixture = new();

    private UpdateRankThresholdHandler CreateHandler()
        => new(_fixture.RankThresholdProvider, _fixture.RankThresholdProvider);

    [Fact]
    public async Task Handle_ValidUpdateBetweenNeighbors_SucceedsAndPersists()
    {
        var handler = CreateHandler();

        // Steel sits between Iron (10) and Mithril (100).
        var result = await handler.Handle(new UpdateRankThresholdCommand(Rank.Steel, 60));

        Assert.Equal(ResultStatus.Success, result.Status);

        var thresholds = await _fixture.RankThresholdProvider.GetThresholdsAsync();
        Assert.Equal(60, thresholds.Single(t => t.Rank == Rank.Steel).PointsRequired);
    }

    [Fact]
    public async Task Handle_RankNotConfigurable_ReturnsFailure()
    {
        var handler = CreateHandler();

        var result = await handler.Handle(new UpdateRankThresholdCommand(Rank.Administrator, 100));

        Assert.Equal(ResultStatus.Failure, result.Status);
        Assert.Contains("has no configurable point threshold", result.Error);
    }

    [Fact]
    public async Task Handle_PointsRequiredNotGreaterThanPreviousRank_ReturnsFailure()
    {
        var handler = CreateHandler();

        // Steel's previous neighbor, Iron, requires 10 points.
        var result = await handler.Handle(new UpdateRankThresholdCommand(Rank.Steel, 10));

        Assert.Equal(ResultStatus.Failure, result.Status);
        Assert.Contains("must be greater than Iron's threshold", result.Error);
    }

    [Fact]
    public async Task Handle_PointsRequiredNotLessThanNextRank_ReturnsFailure()
    {
        var handler = CreateHandler();

        // Steel's next neighbor, Mithril, requires 100 points.
        var result = await handler.Handle(new UpdateRankThresholdCommand(Rank.Steel, 100));

        Assert.Equal(ResultStatus.Failure, result.Status);
        Assert.Contains("must be less than Mithril's threshold", result.Error);
    }

    [Fact]
    public async Task Handle_LowestRankUpdate_OnlyChecksNextNeighbor()
    {
        var handler = CreateHandler();

        // Bronze has no previous neighbor; any value below Iron's (10) is valid, including 0.
        var result = await handler.Handle(new UpdateRankThresholdCommand(Rank.Bronze, 0));

        Assert.Equal(ResultStatus.Success, result.Status);
    }

    [Fact]
    public async Task Handle_HighestConfigurableRankUpdate_OnlyChecksPreviousNeighbor()
    {
        var handler = CreateHandler();

        // Zenyte has no next neighbor among configurable ranks.
        var result = await handler.Handle(new UpdateRankThresholdCommand(Rank.Zenyte, 6000));

        Assert.Equal(ResultStatus.Success, result.Status);
    }
}


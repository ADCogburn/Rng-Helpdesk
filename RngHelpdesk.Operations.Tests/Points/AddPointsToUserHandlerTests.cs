using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Points.Commands;
using RngHelpdesk.Operations.Points;

namespace RngHelpdesk.Operations.Tests.Points;

public class AddPointsToUserHandlerTests
{
    private readonly OperationsTestFixture _fixture = new();

    private AddPointsToUserHandler CreateHandler()
        => new(_fixture.UserRepository, _fixture.EventDispatcher);

    [Fact]
    public async Task Handle_UserDoesNotExist_ReturnsFailure()
    {
        // AggregateNotFoundException isn't a System.AggregateException, so CommandHandler.Execute's
        // AggregateException->NotFound branch never triggers here; it falls through to Fail.
        var handler = CreateHandler();
        var request = new AddPointsToUserRequest { ActingUserId = TestUsers.DefaultActingUserId, UserId = 999, Points = 10, Reason = "Boss kill" };

        var result = await handler.Handle(request);

        Assert.Equal(ResultStatus.Failure, result.Status);
        Assert.Equal("User not found.", result.Error);
    }

    [Fact]
    public async Task Handle_PointsIsNotPositive_ReturnsFailure()
    {
        var user = await _fixture.CreateAndDispatchUserAsync(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        var handler = CreateHandler();
        var request = new AddPointsToUserRequest { ActingUserId = TestUsers.DefaultActingUserId, UserId = user.Id, Points = 0, Reason = "Boss kill" };

        var result = await handler.Handle(request);

        Assert.Equal(ResultStatus.Failure, result.Status);
        Assert.Equal("Points to add must be greater than zero.", result.Error);
    }

    [Fact]
    public async Task Handle_ValidRequest_UpdatesRunningTotalOnReadStores()
    {
        var user = await _fixture.CreateAndDispatchUserAsync(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        var handler = CreateHandler();
        var request = new AddPointsToUserRequest { ActingUserId = TestUsers.DefaultActingUserId, UserId = user.Id, Points = 50, Reason = "Boss kill" };

        var result = await handler.Handle(request);

        Assert.Equal(ResultStatus.Success, result.Status);
        var summary = await _fixture.UserSummaryProjection.GetByIdAsync(user.Id);
        Assert.NotNull(summary);
        Assert.Equal(50, summary!.ClanPoints);
        Assert.Equal(2, await _fixture.PointHistoryProjection.GetCountForUserAsync(user.Id));
    }
}

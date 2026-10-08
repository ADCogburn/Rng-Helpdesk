using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Points.Commands;
using RngHelpdesk.Operations.Points;

namespace RngHelpdesk.Operations.Tests.Points;

public class RemovePointsFromUserHandlerTests
{
    private readonly OperationsTestFixture _fixture = new();

    private RemovePointsFromUserHandler CreateHandler()
        => new(_fixture.UserRepository, _fixture.EventDispatcher);

    [Fact]
    public async Task Handle_UserDoesNotExist_ReturnsFailure()
    {
        var handler = CreateHandler();
        var request = new RemovePointsFromUserRequest { ActingUserId = TestUsers.DefaultActingUserId, UserId = 999, Points = 10, Reason = "Rule violation" };

        var result = await handler.Handle(request);

        Assert.Equal(ResultStatus.Failure, result.Status);
        Assert.Equal("User not found.", result.Error);
    }

    [Fact]
    public async Task Handle_DeductionWouldGoBelowZero_ReturnsFailure()
    {
        var user = await _fixture.CreateAndDispatchUserAsync(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        var handler = CreateHandler();
        var request = new RemovePointsFromUserRequest { ActingUserId = TestUsers.DefaultActingUserId, UserId = user.Id, Points = 10, Reason = "Rule violation" };

        var result = await handler.Handle(request);

        Assert.Equal(ResultStatus.Failure, result.Status);
        Assert.Equal("Cannot deduct clan points below zero.", result.Error);
    }

    [Fact]
    public async Task Handle_ValidRequest_UpdatesRunningTotalOnReadStores()
    {
        var user = await _fixture.CreateAndDispatchUserAsync(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        user.AddClanPoints(TestUsers.DefaultActingUserId, 50, "Boss kill");
        var events = await _fixture.UserRepository.SaveAsync(user);
        _fixture.EventDispatcher.Dispatch(events);

        var handler = CreateHandler();
        var request = new RemovePointsFromUserRequest { ActingUserId = TestUsers.DefaultActingUserId, UserId = user.Id, Points = 20, Reason = "Rule violation" };

        var result = await handler.Handle(request);

        Assert.Equal(ResultStatus.Success, result.Status);
        var summary = await _fixture.UserSummaryProjection.GetByIdAsync(user.Id);
        Assert.NotNull(summary);
        Assert.Equal(30, summary!.ClanPoints);
    }
}

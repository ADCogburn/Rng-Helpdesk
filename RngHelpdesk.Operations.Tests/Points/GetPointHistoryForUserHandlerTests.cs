using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Points.Queries;
using RngHelpdesk.Domain.Users.Events;
using RngHelpdesk.Operations.Points;

namespace RngHelpdesk.Operations.Tests.Points;

public class GetPointHistoryForUserHandlerTests
{
    private readonly OperationsTestFixture _fixture = new();

    private GetPointHistoryForUserHandler CreateHandler()
        => new(_fixture.PointHistoryProjection, _fixture.UserSummaryProjection);

    [Fact]
    public async Task Handle_UserDoesNotExist_ReturnsFailure()
    {
        var handler = CreateHandler();

        var result = await handler.Handle(new GetPointHistoryForUserQuery { UserId = 999 });

        Assert.Equal(ResultStatus.Failure, result.Status);
        Assert.Equal("User not found", result.Error);
    }

    [Fact]
    public async Task Handle_PointHistoryProjectionMissingCreationEvent_ThrowsInvalidOperationException()
    {
        // Seed only the summary projection directly (bypassing the shared dispatcher) to simulate the
        // point-history projection being desynced from a user that does exist in the summary projection.
        var discordAccount = TestUsers.ValidDiscordAccount();
        var userCreatedEvent = UserCreatedEvent.Create(TestUsers.DefaultActingUserId, discordAccount, []);
        _fixture.UserSummaryProjection.Project(userCreatedEvent);

        var handler = CreateHandler();

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(new GetPointHistoryForUserQuery { UserId = userCreatedEvent.UserId }));
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsAccumulatedPointHistory()
    {
        var user = _fixture.CreateAndDispatchUser(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        user.AddClanPoints(TestUsers.DefaultActingUserId, 50, "Boss kill");
        var events = _fixture.UserRepository.Save(user);
        _fixture.EventDispatcher.Dispatch(events);

        var handler = CreateHandler();

        var result = await handler.Handle(new GetPointHistoryForUserQuery { UserId = user.Id });

        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.Equal(2, result.Value!.TotalEventCount);
        Assert.Equal(2, result.Value.Events.Count);
        Assert.Equal(50, result.Value.Events[1].Delta);
    }
}

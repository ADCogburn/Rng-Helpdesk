using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Domain.Users.Events;
using RngHelpdesk.Operations.Users;

namespace RngHelpdesk.Operations.Tests.Users;

public class GetUserLifecycleHistoryHandlerTests
{
    private readonly OperationsTestFixture _fixture = new();

    private GetUserLifecycleHistoryHandler CreateHandler()
        => new(_fixture.UserLifecycleHistoryProjection, _fixture.UserSummaryProjection);

    [Fact]
    public async Task Handle_UserDoesNotExist_ReturnsFailure()
    {
        var handler = CreateHandler();

        var result = await handler.Handle(new GetUserLifecycleHistoryQuery { UserId = 999 });

        Assert.Equal(ResultStatus.Failure, result.Status);
        Assert.Equal("User not found.", result.Error);
    }

    [Fact]
    public async Task Handle_UserNeverDeactivated_ReturnsEmptyHistory()
    {
        var user = await _fixture.CreateAndDispatchUserAsync(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        var handler = CreateHandler();

        var result = await handler.Handle(new GetUserLifecycleHistoryQuery { UserId = user.Id });

        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.Empty(result.Value!.History);
    }

    [Fact]
    public async Task Handle_UserHasLifecycleHistory_ReturnsHistoryFromReadStore()
    {
        // DeactivateUserHandler/ReactivateUserHandler are dead-code shells, so the only way to
        // populate this projection is to feed it the event directly, bypassing the (nonexistent) live handler.
        var user = await _fixture.CreateAndDispatchUserAsync(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        _fixture.UserLifecycleHistoryProjection.Project(UserDeactivatedEvent.Create(TestUsers.DefaultActingUserId, user.Id));
        var handler = CreateHandler();

        var result = await handler.Handle(new GetUserLifecycleHistoryQuery { UserId = user.Id });

        Assert.Equal(ResultStatus.Success, result.Status);
        var item = Assert.Single(result.Value!.History);
        Assert.Equal("Deactivated", item.Action);
    }
}

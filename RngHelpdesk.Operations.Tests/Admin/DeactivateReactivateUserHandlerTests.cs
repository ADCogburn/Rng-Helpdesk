using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Commands;
using RngHelpdesk.Operations.Admin;

namespace RngHelpdesk.Operations.Tests.Admin;

public class DeactivateReactivateUserHandlerTests
{
    private readonly OperationsTestFixture _fixture = new();

    private DeactivateUserHandler CreateDeactivateHandler()
        => new(_fixture.UserRepository, _fixture.EventDispatcher);

    private ReactivateUserHandler CreateReactivateHandler()
        => new(_fixture.UserRepository, _fixture.EventDispatcher);

    [Fact]
    public async Task Deactivate_UserDoesNotExist_ReturnsFailure()
    {
        var handler = CreateDeactivateHandler();

        var result = await handler.Handle(new DeactivateUserRequest(TestUsers.DefaultActingUserId, UserId: 999));

        Assert.Equal(ResultStatus.Failure, result.Status);
        Assert.Equal("User not found.", result.Error);
    }

    [Fact]
    public async Task Deactivate_UserAlreadyDeactivated_ReturnsFailure()
    {
        var user = await _fixture.CreateAndDispatchUserAsync(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        var handler = CreateDeactivateHandler();
        await handler.Handle(new DeactivateUserRequest(TestUsers.DefaultActingUserId, user.Id));

        var result = await handler.Handle(new DeactivateUserRequest(TestUsers.DefaultActingUserId, user.Id));

        Assert.Equal(ResultStatus.Failure, result.Status);
        Assert.Equal("User is already deactivated.", result.Error);
    }

    [Fact]
    public async Task Deactivate_ValidRequest_MarksUserInactive()
    {
        var user = await _fixture.CreateAndDispatchUserAsync(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        var handler = CreateDeactivateHandler();

        var result = await handler.Handle(new DeactivateUserRequest(TestUsers.DefaultActingUserId, user.Id));

        Assert.Equal(ResultStatus.Success, result.Status);
        var summary = await _fixture.UserSummaryProjection.GetByIdAsync(user.Id);
        Assert.NotNull(summary);
        Assert.False(summary!.IsActive);
    }

    [Fact]
    public async Task Reactivate_UserDoesNotExist_ReturnsFailure()
    {
        var handler = CreateReactivateHandler();

        var result = await handler.Handle(new ReactivateUserRequest(TestUsers.DefaultActingUserId, UserId: 999));

        Assert.Equal(ResultStatus.Failure, result.Status);
        Assert.Equal("User not found.", result.Error);
    }

    [Fact]
    public async Task Reactivate_UserAlreadyActive_ReturnsFailure()
    {
        var user = await _fixture.CreateAndDispatchUserAsync(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        var handler = CreateReactivateHandler();

        var result = await handler.Handle(new ReactivateUserRequest(TestUsers.DefaultActingUserId, user.Id));

        Assert.Equal(ResultStatus.Failure, result.Status);
        Assert.Equal("User is already activated.", result.Error);
    }

    [Fact]
    public async Task Reactivate_ValidRequest_MarksUserActive()
    {
        var user = await _fixture.CreateAndDispatchUserAsync(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        var deactivateHandler = CreateDeactivateHandler();
        await deactivateHandler.Handle(new DeactivateUserRequest(TestUsers.DefaultActingUserId, user.Id));
        var reactivateHandler = CreateReactivateHandler();

        var result = await reactivateHandler.Handle(new ReactivateUserRequest(TestUsers.DefaultActingUserId, user.Id));

        Assert.Equal(ResultStatus.Success, result.Status);
        var summary = await _fixture.UserSummaryProjection.GetByIdAsync(user.Id);
        Assert.NotNull(summary);
        Assert.True(summary!.IsActive);
    }
}

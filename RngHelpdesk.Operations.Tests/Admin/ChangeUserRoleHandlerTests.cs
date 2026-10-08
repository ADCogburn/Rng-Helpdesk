using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Contracts.Users.Commands;
using RngHelpdesk.Operations.Admin;

namespace RngHelpdesk.Operations.Tests.Admin;

public class ChangeUserRoleHandlerTests
{
    private readonly OperationsTestFixture _fixture = new();

    private ChangeUserRoleHandler CreateHandler()
        => new(_fixture.UserRoleService, _fixture.UserSummaryProjection, _fixture.EventDispatcher);

    [Fact]
    public async Task Handle_TargetUserDoesNotExist_ReturnsFailure()
    {
        var handler = CreateHandler();

        var result = await handler.Handle(new ChangeUserRoleCommand(TestUsers.DefaultActingUserId, TargetUserId: 999, AppRole.Administrator));

        Assert.Equal(ResultStatus.Failure, result.Status);
        Assert.Equal("User not found.", result.Error);
    }

    [Fact]
    public async Task Handle_NewRoleMatchesCurrentRole_ReturnsFailure()
    {
        var user = await _fixture.CreateAndDispatchUserAsync(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        var handler = CreateHandler();

        var result = await handler.Handle(new ChangeUserRoleCommand(TestUsers.DefaultActingUserId, user.Id, AppRole.Member));

        Assert.Equal(ResultStatus.Failure, result.Status);
        Assert.Equal("User role is already set to the requested role.", result.Error);
    }

    [Fact]
    public async Task Handle_ValidRequest_ChangesRoleAndIsVisibleOnReadStore()
    {
        var user = await _fixture.CreateAndDispatchUserAsync(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        var handler = CreateHandler();

        var result = await handler.Handle(new ChangeUserRoleCommand(TestUsers.DefaultActingUserId, user.Id, AppRole.Administrator));

        Assert.Equal(ResultStatus.Success, result.Status);
        var summary = await _fixture.UserSummaryProjection.GetByIdAsync(user.Id);
        Assert.NotNull(summary);
        Assert.Equal(AppRole.Administrator, summary!.AppRole);
    }

    [Fact]
    public async Task Handle_UserRoleServiceThrows_ReturnsFailureInsteadOfPropagating()
    {
        var user = await _fixture.CreateAndDispatchUserAsync(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        var handler = new ChangeUserRoleHandler(
            new ThrowingUserRoleService(new InvalidOperationException("Concurrency conflict: Expected version 0, but current version is 1.")),
            _fixture.UserSummaryProjection,
            _fixture.EventDispatcher);

        var result = await handler.Handle(new ChangeUserRoleCommand(TestUsers.DefaultActingUserId, user.Id, AppRole.Administrator));

        Assert.Equal(ResultStatus.Failure, result.Status);
        Assert.Equal("Concurrency conflict: Expected version 0, but current version is 1.", result.Error);
    }
}

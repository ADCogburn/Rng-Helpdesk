using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Operations.Users;

namespace RngHelpdesk.Operations.Tests.Users;

public class GetUserByIdHandlerTests
{
    private readonly OperationsTestFixture _fixture = new();

    private GetUserByIdHandler CreateHandler() => new(_fixture.UserSummaryProjection);

    [Fact]
    public async Task Handle_UserDoesNotExist_ReturnsFailure()
    {
        var handler = CreateHandler();

        var result = await handler.Handle(new GetUserByIdQuery(999));

        Assert.Equal(ResultStatus.Failure, result.Status);
        Assert.Equal("User not found.", result.Error);
    }

    [Fact]
    public async Task Handle_UserExists_ReturnsMappedUser()
    {
        var user = await _fixture.CreateAndDispatchUserAsync(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        var handler = CreateHandler();

        var result = await handler.Handle(new GetUserByIdQuery(user.Id));

        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.Equal(user.Id, result.Value!.Id);
        Assert.Equal(TestUsers.DefaultDiscordUsername, result.Value.DiscordAccount.Username);
    }
}

using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Domain.Users;
using RngHelpdesk.Operations.Users;

namespace RngHelpdesk.Operations.Tests.Users;

public class GetUserByRunescapeUsernameHandlerTests
{
    private readonly OperationsTestFixture _fixture = new();

    private GetUserByRunescapeUsernameHandler CreateHandler() => new(_fixture.UserSummaryProjection);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Handle_UsernameIsBlank_ReturnsFailure(string? username)
    {
        var handler = CreateHandler();

        var result = await handler.Handle(new GetUserByRunescapeUsernameQuery(username!));

        Assert.Equal(ResultStatus.Failure, result.Status);
        Assert.Equal("Blank username was requested.", result.Error);
    }

    [Fact]
    public async Task Handle_UsernameHasNoMatchingUser_ReturnsFailure()
    {
        var handler = CreateHandler();

        var result = await handler.Handle(new GetUserByRunescapeUsernameQuery("Zezima"));

        Assert.Equal(ResultStatus.Failure, result.Status);
        Assert.Equal("User not found.", result.Error);
    }

    [Fact]
    public async Task Handle_UsernameLinkedToUser_ReturnsMappedUser()
    {
        var user = _fixture.CreateAndDispatchUser(
            TestUsers.DefaultActingUserId,
            TestUsers.ValidDiscordAccount(),
            [new RunescapeAccount("Zezima")]);
        var handler = CreateHandler();

        var result = await handler.Handle(new GetUserByRunescapeUsernameQuery("Zezima"));

        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.Equal(user.Id, result.Value!.Id);
    }
}

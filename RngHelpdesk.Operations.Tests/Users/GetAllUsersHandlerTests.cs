using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Operations.Users;

namespace RngHelpdesk.Operations.Tests.Users;

public class GetAllUsersHandlerTests
{
    private readonly OperationsTestFixture _fixture = new();

    private GetAllUsersHandler CreateHandler() => new(_fixture.UserSummaryProjection);

    [Fact]
    public async Task Handle_NoUsersExist_ReturnsEmptyResponse()
    {
        var handler = CreateHandler();

        var result = await handler.Handle(new GetAllUsersQuery());

        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.Equal(0, result.Value!.TotalCount);
        Assert.Empty(result.Value.Users);
    }

    [Fact]
    public async Task Handle_UsersExist_ReturnsAllMappedUsers()
    {
        await _fixture.CreateAndDispatchUserAsync(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        await _fixture.CreateAndDispatchUserAsync(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount(discordId: 200, username: "otherUser"));
        var handler = CreateHandler();

        var result = await handler.Handle(new GetAllUsersQuery());

        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.Equal(2, result.Value!.TotalCount);
        Assert.Equal(2, result.Value.Users.Count);
    }
}

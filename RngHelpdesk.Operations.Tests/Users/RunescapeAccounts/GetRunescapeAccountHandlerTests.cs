using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Domain.Users;
using RngHelpdesk.Operations.Users.RunescapeAccounts;

namespace RngHelpdesk.Operations.Tests.Users.RunescapeAccounts;

public class GetRunescapeAccountHandlerTests
{
    private readonly OperationsTestFixture _fixture = new();

    private GetRunescapeAccountHandler CreateHandler() => new(_fixture.UserSummaryProjection);

    [Fact]
    public async Task Handle_UserDoesNotExist_ReturnsFailure()
    {
        var handler = CreateHandler();

        var result = await handler.Handle(new GetRunescapeAccountsQuery { UserId = 999 });

        Assert.Equal(ResultStatus.Failure, result.Status);
        Assert.Equal("User not found.", result.Error);
    }

    [Fact]
    public async Task Handle_UserExists_ReturnsCurrentlyLinkedAccounts()
    {
        var user = _fixture.CreateAndDispatchUser(
            TestUsers.DefaultActingUserId,
            TestUsers.ValidDiscordAccount(),
            [new RunescapeAccount("Zezima")]);
        var handler = CreateHandler();

        var result = await handler.Handle(new GetRunescapeAccountsQuery { UserId = user.Id });

        Assert.Equal(ResultStatus.Success, result.Status);
        var account = Assert.Single(result.Value!.Accounts);
        Assert.Equal("Zezima", account.Username);
    }
}

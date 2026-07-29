using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Commands;
using RngHelpdesk.Domain.Users;
using RngHelpdesk.Operations.Users.RunescapeAccounts;

namespace RngHelpdesk.Operations.Tests.Users.RunescapeAccounts;

public class DelinkRunescapeAccountHandlerTests
{
    private readonly OperationsTestFixture _fixture = new();

    private DelinkRunescapeAccountHandler CreateHandler()
        => new(_fixture.UserRepository, _fixture.EventDispatcher);

    [Fact]
    public async Task Handle_UserDoesNotExist_ReturnsFailure()
    {
        var handler = CreateHandler();

        var result = await handler.Handle(new DelinkRunescapeAccountRequest(TestUsers.DefaultActingUserId, UserId: 999, Username: "Zezima"));

        Assert.Equal(ResultStatus.Failure, result.Status);
        Assert.Equal("User not found.", result.Error);
    }

    [Fact]
    public async Task Handle_UsernameNotLinked_ReturnsFailure()
    {
        var user = await _fixture.CreateAndDispatchUserAsync(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        var handler = CreateHandler();

        var result = await handler.Handle(new DelinkRunescapeAccountRequest(TestUsers.DefaultActingUserId, user.Id, "Zezima"));

        Assert.Equal(ResultStatus.Failure, result.Status);
        Assert.Equal("Runescape account not linked.", result.Error);
    }

    [Fact]
    public async Task Handle_ValidRequest_RemovesAccountAndRecordsHistory()
    {
        var user = await _fixture.CreateAndDispatchUserAsync(
            TestUsers.DefaultActingUserId,
            TestUsers.ValidDiscordAccount(),
            [new RunescapeAccount("Zezima")]);
        var handler = CreateHandler();

        var result = await handler.Handle(new DelinkRunescapeAccountRequest(TestUsers.DefaultActingUserId, user.Id, "Zezima"));

        Assert.Equal(ResultStatus.Success, result.Status);
        var summary = await _fixture.UserSummaryProjection.GetByIdAsync(user.Id);
        Assert.NotNull(summary);
        Assert.Empty(summary!.RunescapeAccounts);
        Assert.Contains(await _fixture.RunescapeAccountHistoryProjection.GetPreviousRunescapeAccountsAsync(user.Id), a => a.Username == "Zezima");
    }
}

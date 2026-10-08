using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Commands;
using RngHelpdesk.Domain.Users;
using RngHelpdesk.Operations.Users.RunescapeAccounts;

namespace RngHelpdesk.Operations.Tests.Users.RunescapeAccounts;

public class RenameRunescapeAccountHandlerTests
{
    private readonly OperationsTestFixture _fixture = new();

    private RenameRunescapeAccountHandler CreateHandler()
        => new(_fixture.UserRepository, _fixture.EventDispatcher);

    [Fact]
    public async Task Handle_UserDoesNotExist_ReturnsFailure()
    {
        var handler = CreateHandler();

        var result = await handler.Handle(new RenameRunescapeAccountRequest(TestUsers.DefaultActingUserId, UserId: 999, OldUsername: "Zezima", NewUsername: "NotZezima"));

        Assert.Equal(ResultStatus.Failure, result.Status);
        Assert.Equal("User not found.", result.Error);
    }

    [Fact]
    public async Task Handle_OldUsernameNotLinked_ReturnsFailure()
    {
        var user = await _fixture.CreateAndDispatchUserAsync(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        var handler = CreateHandler();

        var result = await handler.Handle(new RenameRunescapeAccountRequest(TestUsers.DefaultActingUserId, user.Id, "Zezima", "NotZezima"));

        Assert.Equal(ResultStatus.Failure, result.Status);
        Assert.Equal("Runescape account not found.", result.Error);
    }

    [Fact]
    public async Task Handle_NewUsernameCollidesWithExistingAccount_ReturnsFailure()
    {
        var user = await _fixture.CreateAndDispatchUserAsync(
            TestUsers.DefaultActingUserId,
            TestUsers.ValidDiscordAccount(),
            [new RunescapeAccount("Zezima"), new RunescapeAccount("OtherAccount")]);
        var handler = CreateHandler();

        var result = await handler.Handle(new RenameRunescapeAccountRequest(TestUsers.DefaultActingUserId, user.Id, "Zezima", "OtherAccount"));

        Assert.Equal(ResultStatus.Failure, result.Status);
        Assert.Equal("Runescape account already exists.", result.Error);
    }

    [Fact]
    public async Task Handle_ValidRequest_RenamesAccountAndRecordsHistory()
    {
        var user = await _fixture.CreateAndDispatchUserAsync(
            TestUsers.DefaultActingUserId,
            TestUsers.ValidDiscordAccount(),
            [new RunescapeAccount("Zezima")]);
        var handler = CreateHandler();

        var result = await handler.Handle(new RenameRunescapeAccountRequest(TestUsers.DefaultActingUserId, user.Id, "Zezima", "NotZezima"));

        Assert.Equal(ResultStatus.Success, result.Status);
        var summary = await _fixture.UserSummaryProjection.GetByIdAsync(user.Id);
        Assert.NotNull(summary);
        Assert.Contains(summary!.RunescapeAccounts, a => a.Username == "NotZezima");
        Assert.DoesNotContain(summary.RunescapeAccounts, a => a.Username == "Zezima");
        Assert.Contains(await _fixture.RunescapeAccountHistoryProjection.GetHistoryAsync(user.Id), h => h.OldUsername == "Zezima" && h.NewUsername == "NotZezima");
    }
}

using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Domain.Users;
using RngHelpdesk.Operations.Users.RunescapeAccounts;

namespace RngHelpdesk.Operations.Tests.Users.RunescapeAccounts;

public class LinkRunescapeAccountHandlerTests
{
    private readonly OperationsTestFixture _fixture = new();

    private LinkRunescapeAccountHandler CreateHandler(FakeLinkRunescapeAccountValidator validator)
        => new(_fixture.UserRepository, _fixture.EventDispatcher, validator);

    [Fact]
    public async Task Handle_ValidatorReturnsInvalid_ReturnsFailureWithJoinedErrors()
    {
        var handler = CreateHandler(FakeLinkRunescapeAccountValidator.Failing("Invalid RuneScape username.", "Too long."));

        var result = await handler.Handle(new LinkRunescapeAccountRequest(TestUsers.DefaultActingUserId, UserId: 1, Username: "!!!"));

        Assert.Equal(ResultStatus.Failure, result.Status);
        Assert.Equal("Invalid RuneScape username.; Too long.", result.Error);
    }

    [Fact]
    public async Task Handle_ValidRequestUserDoesNotExist_ReturnsFailure()
    {
        var handler = CreateHandler(FakeLinkRunescapeAccountValidator.Passing());

        var result = await handler.Handle(new LinkRunescapeAccountRequest(TestUsers.DefaultActingUserId, UserId: 999, Username: "Zezima"));

        Assert.Equal(ResultStatus.Failure, result.Status);
        Assert.Equal("User not found.", result.Error);
    }

    [Fact]
    public async Task Handle_UsernameAlreadyLinked_ReturnsFailure()
    {
        var user = await _fixture.CreateAndDispatchUserAsync(
            TestUsers.DefaultActingUserId,
            TestUsers.ValidDiscordAccount(),
            [new RunescapeAccount("Zezima")]);
        var handler = CreateHandler(FakeLinkRunescapeAccountValidator.Passing());

        var result = await handler.Handle(new LinkRunescapeAccountRequest(TestUsers.DefaultActingUserId, user.Id, "Zezima"));

        Assert.Equal(ResultStatus.Failure, result.Status);
        Assert.Equal("Runescape account already linked.", result.Error);
    }

    [Fact]
    public async Task Handle_ValidRequest_LinksAccountAndIsVisibleOnReadStore()
    {
        var user = await _fixture.CreateAndDispatchUserAsync(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        var handler = CreateHandler(FakeLinkRunescapeAccountValidator.Passing());

        var result = await handler.Handle(new LinkRunescapeAccountRequest(TestUsers.DefaultActingUserId, user.Id, "Zezima"));

        Assert.Equal(ResultStatus.Success, result.Status);
        var summary = await _fixture.UserSummaryProjection.GetByIdAsync(user.Id);
        Assert.NotNull(summary);
        Assert.Contains(summary!.RunescapeAccounts, a => a.Username == "Zezima");
    }
}

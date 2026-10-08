using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Domain.Users;
using RngHelpdesk.Operations.Users.RunescapeAccounts;

namespace RngHelpdesk.Operations.Tests.Users.RunescapeAccounts;

public class GetPreviousRunescapeAccountsHandlerTests
{
    private readonly OperationsTestFixture _fixture = new();

    private GetPreviousRunescapeAccountsHandler CreateHandler() => new(_fixture.RunescapeAccountHistoryProjection);

    [Fact]
    public async Task Handle_UserIdHasNoDelinkOrRenameHistory_ReturnsEmptyAccounts()
    {
        var handler = CreateHandler();

        var result = await handler.Handle(new GetPreviousRunescapeAccountsQuery(999));

        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.Empty(result.Value!.Accounts);
    }

    [Fact]
    public async Task Handle_UserIdHasDelinkedAccounts_ReturnsPreviousAccounts()
    {
        var user = await _fixture.CreateAndDispatchUserAsync(
            TestUsers.DefaultActingUserId,
            TestUsers.ValidDiscordAccount(),
            [new RunescapeAccount("Zezima")]);

        user.RemoveRunescapeAccount(TestUsers.DefaultActingUserId, "Zezima");
        var events = await _fixture.UserRepository.SaveAsync(user);
        _fixture.EventDispatcher.Dispatch(events);

        var handler = CreateHandler();

        var result = await handler.Handle(new GetPreviousRunescapeAccountsQuery(user.Id));

        Assert.Equal(ResultStatus.Success, result.Status);
        var account = Assert.Single(result.Value!.Accounts);
        Assert.Equal("Zezima", account.Username);
    }
}

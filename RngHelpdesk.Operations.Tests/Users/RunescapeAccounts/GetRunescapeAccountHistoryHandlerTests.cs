using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Contracts.Users.Views;
using RngHelpdesk.Domain.Users;
using RngHelpdesk.Operations.Users.RunescapeAccounts;

namespace RngHelpdesk.Operations.Tests.Users.RunescapeAccounts;

public class GetRunescapeAccountHistoryHandlerTests
{
    private readonly OperationsTestFixture _fixture = new();

    private GetRunescapeAccountHistoryHandler CreateHandler() => new(_fixture.RunescapeAccountHistoryProjection);

    [Fact]
    public async Task Handle_UserIdHasNoHistory_ReturnsEmptyHistory()
    {
        var handler = CreateHandler();

        var result = await handler.Handle(new GetRunescapeAccountHistoryQuery(999));

        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.Empty(result.Value!.History);
    }

    [Fact]
    public async Task Handle_UserIdHasLinkRenameDelinkHistory_ReturnsFullEventHistory()
    {
        var user = _fixture.CreateAndDispatchUser(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());

        user.AddRunescapeAccount(TestUsers.DefaultActingUserId, "Zezima");
        _fixture.EventDispatcher.Dispatch(_fixture.UserRepository.Save(user));

        user.RenameRunescapeAccount(TestUsers.DefaultActingUserId, "Zezima", "NotZezima");
        _fixture.EventDispatcher.Dispatch(_fixture.UserRepository.Save(user));

        user.RemoveRunescapeAccount(TestUsers.DefaultActingUserId, "NotZezima");
        _fixture.EventDispatcher.Dispatch(_fixture.UserRepository.Save(user));

        var handler = CreateHandler();

        var result = await handler.Handle(new GetRunescapeAccountHistoryQuery(user.Id));

        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.Equal(3, result.Value!.History.Count);
        Assert.Equal(RunescapeAccountChangeType.Linked, result.Value.History[0].ChangeType);
        Assert.Equal(RunescapeAccountChangeType.Renamed, result.Value.History[1].ChangeType);
        Assert.Equal(RunescapeAccountChangeType.Delinked, result.Value.History[2].ChangeType);
    }
}

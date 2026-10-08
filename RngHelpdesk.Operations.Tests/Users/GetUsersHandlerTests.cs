using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Domain.Users.Events;
using RngHelpdesk.Operations.Users;

namespace RngHelpdesk.Operations.Tests.Users;

public class GetUsersHandlerTests
{
    private const ulong PhantomUserId = 12345;

    private readonly OperationsTestFixture _fixture = new();

    private GetUsersHandler CreateHandler()
        => new(_fixture.RunescapeAccountHistoryProjection, _fixture.UserSummaryProjection);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Handle_HistoricalUsernameIsBlank_ReturnsFailure(string? username)
    {
        var handler = CreateHandler();

        var result = await handler.Handle(new GetUsersByHistoricalRunescapeUsernameQuery(username!));

        Assert.Equal(ResultStatus.Failure, result.Status);
        Assert.Equal("Blank username was requested.", result.Error);
    }

    [Fact]
    public async Task Handle_HistoricalUsernameHasNoMatchingUsers_ReturnsFailure()
    {
        var handler = CreateHandler();

        var result = await handler.Handle(new GetUsersByHistoricalRunescapeUsernameQuery("Zezima"));

        Assert.Equal(ResultStatus.Failure, result.Status);
        Assert.Equal("User not found.", result.Error);
    }

    [Fact]
    public async Task Handle_MatchingUserIdNoLongerResolves_IsSilentlySkipped()
    {
        var user = await _fixture.CreateAndDispatchUserAsync(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        user.AddRunescapeAccount(TestUsers.DefaultActingUserId, "Zezima");
        var events = await _fixture.UserRepository.SaveAsync(user);
        _fixture.EventDispatcher.Dispatch(events);

        // Simulate a summary-projection/history-projection desync: a second userId that historically
        // used "Zezima" but was never (or is no longer) present in the summary projection.
        _fixture.RunescapeAccountHistoryProjection.Project(
            RunescapeAccountLinkedEvent.Create(TestUsers.DefaultActingUserId, PhantomUserId, "Zezima"));

        var handler = CreateHandler();

        var result = await handler.Handle(new GetUsersByHistoricalRunescapeUsernameQuery("Zezima"));

        Assert.Equal(ResultStatus.Success, result.Status);
        var returned = Assert.Single(result.Value!.Users);
        Assert.Equal(user.Id, returned.Id);
    }
}

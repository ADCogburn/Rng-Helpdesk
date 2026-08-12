using Npgsql;
using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Common.Ranks;
using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Contracts.Users.Commands;
using RngHelpdesk.Domain.Users;
using RngHelpdesk.Infrastructure.Common;
using RngHelpdesk.Infrastructure.Persistence.EventStore;
using RngHelpdesk.Infrastructure.Persistence.Points;
using RngHelpdesk.Infrastructure.Users;
using RngHelpdesk.Operations.Admin;
using RngHelpdesk.Operations.Services;

namespace RngHelpdesk.Infrastructure.Tests;

/// <summary>
/// Representative of the "bypasses the aggregate" shape -- ChangeUserRoleHandler appends an
/// IApplicationEvent directly via IUserRoleService/PostgresEventStore rather than going through
/// User's behavior methods. This exists specifically as a regression test: it was written to
/// exercise UserRoleService against real Postgres and caught a live bug where a role-changed
/// user's "User" stream mixed an IApplicationEvent in with the aggregate's own domain events, and
/// PostgresUserRepository.GetByIdAsync crashed trying to deserialize it as an IDomainEvent --
/// see ADR 0006 for the fix (StoredEventDeserializer/AggregateRoot now work in terms of IEvent).
/// RngHelpdesk.Operations.Tests' fixture never caught this because it wires IUserRoleService to
/// InMemoryEventStore but IUserRepository to a separate, independent InMemUserRepository -- the two
/// never share a stream in-memory the way the Postgres-backed singletons do in Program.cs.
/// </summary>
public sealed class ChangeUserRoleHandlerIntegrationTests : IClassFixture<MigrationFixture>
{
    private readonly MigrationFixture _fixture;

    public ChangeUserRoleHandlerIntegrationTests(MigrationFixture fixture)
    {
        _fixture = fixture;
    }

    private static ulong NewStreamId() => (ulong)Random.Shared.NextInt64(1, long.MaxValue);

    [Fact]
    public async Task Handle_ValidRequest_ChangesRoleAndLeavesUserStreamReloadable()
    {
        var dataSource = NpgsqlDataSource.Create(_fixture.ConnectionString);
        var registry = EventStoreRegistration.CreateRegistry();
        var eventStore = new PostgresEventStore(dataSource, registry);
        var repo = new PostgresUserRepository(eventStore, registry);

        var rankResolver = new RankResolver(await new InMemoryRankThresholdProvider().GetThresholdsAsync());
        var userSummaryProjection = new UserSummaryProjection(rankResolver);
        var dispatcher = new InMemEventDispatcher([userSummaryProjection]);
        var userRoleService = new UserRoleService(eventStore);

        var userId = NewStreamId();
        var user = User.Create(
            actingUserId: 1,
            discordAccount: new DiscordAccount(userId, "ChangeRoleIntegrationUser"),
            runescapeAccounts: []);

        var createdEvents = await repo.SaveAsync(user);
        dispatcher.Dispatch(createdEvents);

        var handler = new ChangeUserRoleHandler(userRoleService, userSummaryProjection, dispatcher);

        var result = await handler.Handle(new ChangeUserRoleCommand(ActingUserId: 1, TargetUserId: userId, AppRole.Administrator));

        Assert.Equal(ResultStatus.Success, result.Status);

        var summary = await userSummaryProjection.GetByIdAsync(userId);
        Assert.NotNull(summary);
        Assert.Equal(AppRole.Administrator, summary!.AppRole);

        // The real regression check: reloading the aggregate must not throw, even though the "User"
        // stream now also contains the UserAppRoleChangedEvent this handler just appended.
        var reloaded = await repo.GetByIdAsync(userId);
        Assert.Equal(userId, reloaded.Id);
        Assert.True(reloaded.IsActive);

        // And the stream must still be usable for further aggregate mutations afterward.
        reloaded.Deactivate(actingUserId: 1);
        var deactivatedEvents = await repo.SaveAsync(reloaded);
        Assert.Single(deactivatedEvents);

        await dataSource.DisposeAsync();
    }
}

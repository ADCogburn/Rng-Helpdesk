using Npgsql;
using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Points.Commands;
using RngHelpdesk.Domain.Users;
using RngHelpdesk.Infrastructure.Common;
using RngHelpdesk.Infrastructure.Persistence.EventStore;
using RngHelpdesk.Infrastructure.Users;
using RngHelpdesk.Operations.Points;

namespace RngHelpdesk.Infrastructure.Tests;

/// <summary>
/// Representative of the "aggregate-backed command handler" shape (load via IUserRepository,
/// mutate the aggregate, SaveAsync, dispatch) -- proves the handler round-trips a real domain
/// event through Postgres, which RngHelpdesk.Operations.Tests can't: its fixture's in-memory
/// fakes never serialize anything, so they can't catch a persistence-layer bug like a broken
/// JSON round-trip for a specific event type.
/// </summary>
public sealed class AddPointsToUserHandlerIntegrationTests : IClassFixture<MigrationFixture>
{
    private readonly MigrationFixture _fixture;

    public AddPointsToUserHandlerIntegrationTests(MigrationFixture fixture)
    {
        _fixture = fixture;
    }

    private (NpgsqlDataSource dataSource, PostgresEventStore store, PostgresUserRepository repo) CreateRepo()
    {
        var dataSource = NpgsqlDataSource.Create(_fixture.ConnectionString);
        var registry = EventStoreRegistration.CreateRegistry();
        var store = new PostgresEventStore(dataSource, registry);
        var repo = new PostgresUserRepository(store, registry);

        return (dataSource, store, repo);
    }

    private static ulong NewStreamId() => (ulong)Random.Shared.NextInt64(1, long.MaxValue);

    [Fact]
    public async Task Handle_ValidRequest_PersistsClanPointsChangedEventThroughPostgres()
    {
        var (dataSource, store, repo) = CreateRepo();
        var userId = NewStreamId();

        var user = User.Create(
            actingUserId: 1,
            discordAccount: new DiscordAccount(userId, "AddPointsIntegrationUser"),
            runescapeAccounts: []);

        await repo.SaveAsync(user);

        var handler = new AddPointsToUserHandler(repo, new InMemEventDispatcher([]));
        var request = new AddPointsToUserRequest { ActingUserId = 1, UserId = userId, Points = 50, Reason = "Boss kill" };

        var result = await handler.Handle(request);

        Assert.Equal(ResultStatus.Success, result.Status);

        var reloaded = await repo.GetByIdAsync(userId);
        Assert.Equal(2, reloaded.Version);

        var stored = await store.LoadStreamAsync("User", userId);
        var pointsEventRow = Assert.Single(stored, e => e.EventType == "Points.PointsChanged");
        var pointsEvent = (ClanPointsChangedEvent)StoredEventDeserializer.Deserialize(pointsEventRow, typeof(ClanPointsChangedEvent));

        Assert.Equal(50, pointsEvent.Delta);
        Assert.Equal("Boss kill", pointsEvent.Reason);

        await dataSource.DisposeAsync();
    }
}

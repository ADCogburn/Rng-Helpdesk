using Npgsql;
using RngHelpdesk.Domain.Common;
using RngHelpdesk.Domain.Users.Events;
using RngHelpdesk.Infrastructure.Persistence.EventStore;

namespace RngHelpdesk.Infrastructure.Tests;

public sealed class EventStoreConcurrencyTests : IClassFixture<MigrationFixture>
{
    private readonly MigrationFixture _fixture;

    public EventStoreConcurrencyTests(MigrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AppendAsync_ConcurrentWritersToSameStream_OneSucceedsAndOneThrowsConcurrencyConflict()
    {
        var dataSource = NpgsqlDataSource.Create(_fixture.ConnectionString);
        var registry = EventStoreRegistration.CreateRegistry();
        var store = new PostgresEventStore(dataSource, registry);

        const string streamType = "User";
        var streamId = (ulong)Random.Shared.NextInt64(1, long.MaxValue);

        IReadOnlyList<IEvent> Events() => new IEvent[] { UserDeactivatedEvent.Create(actingUserId: 1, userId: streamId) };

        var results = await Task.WhenAll(
            AppendAndCapture(store, streamType, streamId, Events()),
            AppendAndCapture(store, streamType, streamId, Events()));

        Assert.Single(results, r => r is null);
        Assert.Single(results, r => r is ConcurrencyConflictException);

        var stream = await store.LoadStreamAsync(streamType, streamId);
        Assert.Single(stream);
        Assert.Equal(1, stream[0].StreamVersion);

        await dataSource.DisposeAsync();
    }

    private static async Task<Exception?> AppendAndCapture(
        IEventStore store,
        string streamType,
        ulong streamId,
        IReadOnlyList<IEvent> events)
    {
        try
        {
            await store.AppendAsync(streamType, streamId, expectedVersion: 0, events, new EventStoreMetadata());
            return null;
        }
        catch (ConcurrencyConflictException ex)
        {
            return ex;
        }
    }
}

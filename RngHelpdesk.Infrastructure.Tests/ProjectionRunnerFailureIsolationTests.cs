using Npgsql;
using RngHelpdesk.Domain.Common;
using RngHelpdesk.Domain.Users.Events;
using RngHelpdesk.Infrastructure.Common;
using RngHelpdesk.Infrastructure.Persistence.EventStore;
using RngHelpdesk.Infrastructure.Persistence.Projections;

namespace RngHelpdesk.Infrastructure.Tests;

public sealed class ProjectionRunnerFailureIsolationTests : IClassFixture<MigrationFixture>
{
    private readonly MigrationFixture _fixture;

    public ProjectionRunnerFailureIsolationTests(MigrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task RunAsync_OneProjectionThrows_OtherProjectionStillReceivesEveryEvent()
    {
        var dataSource = NpgsqlDataSource.Create(_fixture.ConnectionString);
        var registry = EventStoreRegistration.CreateRegistry();
        var eventStore = new PostgresEventStore(dataSource, registry);
        var checkpoints = new PostgresProjectionCheckpointStore(dataSource);

        const string streamType = "User";
        var streamIds = new[]
        {
            (ulong)Random.Shared.NextInt64(1, long.MaxValue),
            (ulong)Random.Shared.NextInt64(1, long.MaxValue),
            (ulong)Random.Shared.NextInt64(1, long.MaxValue)
        };

        foreach (var streamId in streamIds)
        {
            await eventStore.AppendAsync(
                streamType,
                streamId,
                expectedVersion: 0,
                new IEvent[] { UserDeactivatedEvent.Create(actingUserId: 1, userId: streamId) },
                new EventStoreMetadata());
        }

        var throwing = new ThrowingProjection();
        var recording = new RecordingProjection();

        var runner = new ProjectionRunner(eventStore, registry, checkpoints, new object[] { throwing, recording });

        await runner.RunAsync();

        Assert.Equal(streamIds.OrderBy(x => x), recording.ReceivedUserIds.OrderBy(x => x));

        await dataSource.DisposeAsync();
    }

    private sealed class ThrowingProjection : IProjectionHandler<UserDeactivatedEvent>, IProjectionState
    {
        public bool IsEmpty => true;

        public void Project(UserDeactivatedEvent @event) => throw new InvalidOperationException("Simulated projection failure.");
    }

    private sealed class RecordingProjection : IProjectionHandler<UserDeactivatedEvent>, IProjectionState
    {
        public List<ulong> ReceivedUserIds { get; } = new();

        public bool IsEmpty => true;

        public void Project(UserDeactivatedEvent @event) => ReceivedUserIds.Add(@event.UserId);
    }
}

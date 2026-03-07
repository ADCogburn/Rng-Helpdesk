using Npgsql;
using RngHelpdesk.Domain.Common;
using System.Text.Json;

namespace RngHelpdesk.Infrastructure.Persistence.EventStore;

public sealed class PostgresEventStore : IEventStore
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly EventTypeRegistry _registry;

    public PostgresEventStore(
        NpgsqlDataSource dataSource,
        EventTypeRegistry registry)
    {
        this._dataSource = dataSource;
        this._registry = registry;
    }

    /// <summary>
    /// Loads the full ordered history for a single aggregate.
    /// </summary>
    /// <param name="streamType"></param>
    /// <param name="streamId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<IReadOnlyList<StoredEvent>> LoadStreamAsync(
        string streamType,
        int streamId,
        CancellationToken ct = default)
    {
        var results = new List<StoredEvent>();

        await using var conn = await _dataSource.OpenConnectionAsync(ct);
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = await LoadSqlAsync("LoadStream.sql", ct);

        cmd.Parameters.AddWithValue("streamType", streamType);
        cmd.Parameters.AddWithValue("streamId", streamId);

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            results.Add(ReadStoredEvent(reader));
        }

        return results;
    }

    /// <summary>
    /// Loads all of the events after a given point in time globally. Should only be used by Projections.
    /// </summary>
    /// <param name="globalPosition"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<IReadOnlyList<StoredEvent>> LoadFromPositionAsync(
        long globalPosition,
        CancellationToken ct = default)
    {
        var results = new List<StoredEvent>();

        await using var conn = await _dataSource.OpenConnectionAsync(ct);
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = await LoadSqlAsync("LoadFromPosition.sql", ct);

        cmd.Parameters.AddWithValue("position", globalPosition);

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            results.Add(ReadStoredEvent(reader));
        }

        return results;
    }

    /// <summary>
    /// Appends current domain event into the stream, confirming the proper versioning (so there isn't overwriting on simultaneous requests).
    /// </summary>
    /// <param name="streamType"></param>
    /// <param name="streamId"></param>
    /// <param name="expectedVersion"></param>
    /// <param name="events"></param>
    /// <param name="metadata"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task AppendAsync(string streamType,
        int streamId,
        int expectedVersion,
        IReadOnlyList<IDomainEvent> events,
        EventStoreMetadata metadata,
        CancellationToken ct = default)
    {
        if (events.Count == 0)
            return;

        await using var conn = await _dataSource.OpenConnectionAsync(ct);
        await using var tx = await conn.BeginTransactionAsync(ct);

        // Ensure stream row exists
        await EnsureStreamAsync(conn, tx, streamType, streamId, ct);

        // Lock stream and read current version
        int currentVersion;

        await using (var versionCmd = conn.CreateCommand())
        {
            versionCmd.Transaction = tx;
            versionCmd.CommandText = await LoadSqlAsync("AppendEvents.sql", ct);

            versionCmd.Parameters.AddWithValue("streamType", streamType);
            versionCmd.Parameters.AddWithValue("streamId", streamId);

            currentVersion = (int)(await versionCmd.ExecuteScalarAsync(ct))!;
        }

        if (currentVersion != expectedVersion)
            throw new InvalidOperationException(
                $"Concurrency conflict. Expected {expectedVersion}, but was {currentVersion}");

        var nextVersion = currentVersion;

        foreach (var ev in events)
        {
            nextVersion++;

            await using var insert = conn.CreateCommand();
            insert.Transaction = tx;

            insert.CommandText = @"
                insert into eventstore.event_store
                (""StreamType"", ""StreamId"", ""StreamVersion"", ""EventType"", ""EventSchemaVer"",
                 ""OccurredUtc"", ""RecordedUtc"", ""Payload"", ""Metadata"")
                values
                (@streamType, @streamId, @streamVersion, @eventType, @schemaVer,
                 @occurredUtc, @recordedUtc, @payload, @metadata)";

            insert.Parameters.AddWithValue("streamType", streamType);
            insert.Parameters.AddWithValue("streamId", streamId);
            insert.Parameters.AddWithValue("streamVersion", nextVersion);
            insert.Parameters.AddWithValue("eventType", _registry.GetName(ev.GetType()));
            insert.Parameters.AddWithValue("schemaVer", 1);
            insert.Parameters.AddWithValue("occurredUtc", ev.OccurredAt);
            insert.Parameters.AddWithValue("recordedUtc", DateTime.UtcNow);
            insert.Parameters.AddWithValue("payload",
                JsonSerializer.Serialize(ev, ev.GetType()));
            insert.Parameters.AddWithValue("metadata",
                JsonSerializer.Serialize(metadata));

            await insert.ExecuteNonQueryAsync(ct);
        }

        // Update stream version
        await using (var update = conn.CreateCommand())
        {
            update.Transaction = tx;
            update.CommandText = @"
                update eventstore.event_streams
                set ""CurrentVersion"" = @ver, ""UpdatedUtc"" = now()
                where ""StreamType"" = @streamType and ""StreamId"" = @streamId";

            update.Parameters.AddWithValue("ver", nextVersion);
            update.Parameters.AddWithValue("streamType", streamType);
            update.Parameters.AddWithValue("streamId", streamId);

            await update.ExecuteNonQueryAsync(ct);
        }

        await tx.CommitAsync(ct);
    }

    private static async Task EnsureStreamAsync(
        NpgsqlConnection conn,
        NpgsqlTransaction tx,
        string streamType,
        int streamId,
        CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;

        cmd.CommandText = @"
            insert into eventstore.event_streams
            (""StreamType"", ""StreamId"", ""CurrentVersion"", ""CreatedUtc"", ""UpdatedUtc"")
            values (@t, @i, 0, now(), now())
            on conflict do nothing";

        cmd.Parameters.AddWithValue("t", streamType);
        cmd.Parameters.AddWithValue("i", streamId);

        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static StoredEvent ReadStoredEvent(NpgsqlDataReader r)
        => new(
            r.GetInt64(r.GetOrdinal("GlobalPosition")),
            r.GetString(r.GetOrdinal("StreamType")),
            r.GetInt32(r.GetOrdinal("StreamId")),
            r.GetInt32(r.GetOrdinal("StreamVersion")),
            r.GetString(r.GetOrdinal("EventType")),
            r.GetInt32(r.GetOrdinal("EventSchemaVer")),
            r.GetDateTime(r.GetOrdinal("OccurredUtc")),
            r.GetString(r.GetOrdinal("Payload")),
            r.GetString(r.GetOrdinal("Metadata"))
        );

    private static Task<string> LoadSqlAsync(string file, CancellationToken ct)
    {
        var path = Path.Combine(
            AppContext.BaseDirectory,
            "Persistence",
            "Sql",
            file);

        return File.ReadAllTextAsync(path, ct);
    }

}
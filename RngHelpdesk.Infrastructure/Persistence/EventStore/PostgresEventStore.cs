using Npgsql;
using NpgsqlTypes;
using RngHelpdesk.Domain.Common;
using System.Text.Json;

namespace RngHelpdesk.Infrastructure.Persistence.EventStore;

public sealed class PostgresEventStore : IEventStore
{
    private const string ConcurrencyIndexName = "IX_event_store_StreamType_StreamId_StreamVersion";

    private readonly NpgsqlDataSource _dataSource;
    private readonly EventTypeRegistry _registry;

    public PostgresEventStore(NpgsqlDataSource dataSource, EventTypeRegistry registry)
    {
        _dataSource = dataSource;
        _registry = registry;
    }

    public async Task<IReadOnlyList<StoredEvent>> LoadStreamAsync(
        string streamType,
        ulong streamId,
        CancellationToken ct = default)
    {
        var results = new List<StoredEvent>();

        await using var conn = await _dataSource.OpenConnectionAsync(ct);
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = """
            select "GlobalPosition", "StreamType", "StreamId", "StreamVersion", "EventType",
                   "EventSchemaVer", "OccurredUtc", "Payload", "Metadata"
            from eventstore.event_store
            where "StreamType" = @streamType and "StreamId" = @streamId
            order by "StreamVersion"
            """;

        cmd.Parameters.AddWithValue("streamType", streamType);
        cmd.Parameters.AddWithValue("streamId", (long)streamId);

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            results.Add(ReadStoredEvent(reader));
        }

        return results;
    }

    public async Task<IReadOnlyList<StoredEvent>> LoadFromPositionAsync(
        long globalPosition,
        CancellationToken ct = default)
    {
        var results = new List<StoredEvent>();

        await using var conn = await _dataSource.OpenConnectionAsync(ct);
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = """
            select "GlobalPosition", "StreamType", "StreamId", "StreamVersion", "EventType",
                   "EventSchemaVer", "OccurredUtc", "Payload", "Metadata"
            from eventstore.event_store
            where "GlobalPosition" > @position
            order by "GlobalPosition"
            """;

        cmd.Parameters.AddWithValue("position", globalPosition);

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            results.Add(ReadStoredEvent(reader));
        }

        return results;
    }

    /// <summary>
    /// Appends events to a stream, inserting optimistically at StreamVersion = expectedVersion + n.
    /// A concurrent writer that already claimed one of those versions trips the unique index on
    /// (StreamType, StreamId, StreamVersion), which is translated into a
    /// <see cref="ConcurrencyConflictException"/> rather than being detected via an explicit lock/read.
    /// </summary>
    public async Task AppendAsync(
        string streamType,
        ulong streamId,
        int expectedVersion,
        IReadOnlyList<IEvent> events,
        EventStoreMetadata metadata,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(streamType))
            throw new ArgumentException("Stream type is required.", nameof(streamType));

        if (events.Count == 0)
            return;

        await using var conn = await _dataSource.OpenConnectionAsync(ct);
        await using var tx = await conn.BeginTransactionAsync(ct);

        var nextVersion = expectedVersion;

        try
        {
            foreach (var e in events)
            {
                nextVersion++;

                await using var insert = conn.CreateCommand();
                insert.Transaction = tx;

                insert.CommandText = """
                    insert into eventstore.event_store
                    ("StreamType", "StreamId", "StreamVersion", "EventType", "EventSchemaVer",
                     "OccurredUtc", "RecordedUtc", "Payload", "Metadata")
                    values
                    (@streamType, @streamId, @streamVersion, @eventType, @schemaVer,
                     @occurredUtc, @recordedUtc, @payload, @metadata)
                    """;

                insert.Parameters.AddWithValue("streamType", streamType);
                insert.Parameters.AddWithValue("streamId", (long)streamId);
                insert.Parameters.AddWithValue("streamVersion", nextVersion);
                insert.Parameters.AddWithValue("eventType", _registry.GetName(e.GetType()));
                insert.Parameters.AddWithValue("schemaVer", 1);
                insert.Parameters.Add("occurredUtc", NpgsqlDbType.TimestampTz).Value = e.OccurredAt;
                insert.Parameters.Add("recordedUtc", NpgsqlDbType.TimestampTz).Value = DateTimeOffset.UtcNow;
                insert.Parameters.Add("payload", NpgsqlDbType.Jsonb).Value = JsonSerializer.Serialize(e, e.GetType());
                insert.Parameters.Add("metadata", NpgsqlDbType.Jsonb).Value = JsonSerializer.Serialize(metadata);

                await insert.ExecuteNonQueryAsync(ct);
            }

            await tx.CommitAsync(ct);
        }
        catch (PostgresException ex) when (IsConcurrencyConflict(ex))
        {
            await tx.RollbackAsync(CancellationToken.None);

            throw new ConcurrencyConflictException(streamType, streamId, expectedVersion, ex);
        }
    }

    public async Task<bool> HasAnyStreamsAsync(string streamType, CancellationToken ct = default)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(ct);
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = """
            select exists(
                select 1 from eventstore.event_store
                where "StreamType" = @streamType
            )
            """;
        cmd.Parameters.AddWithValue("streamType", streamType);

        return (bool)(await cmd.ExecuteScalarAsync(ct))!;
    }

    private static bool IsConcurrencyConflict(PostgresException ex)
        => ex.SqlState == PostgresErrorCodes.UniqueViolation
           && ex.ConstraintName == ConcurrencyIndexName;

    private static StoredEvent ReadStoredEvent(NpgsqlDataReader r)
        => new(
            r.GetInt64(r.GetOrdinal("GlobalPosition")),
            r.GetString(r.GetOrdinal("StreamType")),
            (ulong)r.GetInt64(r.GetOrdinal("StreamId")),
            r.GetInt32(r.GetOrdinal("StreamVersion")),
            r.GetString(r.GetOrdinal("EventType")),
            r.GetInt32(r.GetOrdinal("EventSchemaVer")),
            r.GetFieldValue<DateTimeOffset>(r.GetOrdinal("OccurredUtc")),
            r.GetString(r.GetOrdinal("Payload")),
            r.GetString(r.GetOrdinal("Metadata"))
        );
}

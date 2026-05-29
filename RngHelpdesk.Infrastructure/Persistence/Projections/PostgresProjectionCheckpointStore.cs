using Npgsql;

namespace RngHelpdesk.Infrastructure.Persistence.Projections;

public sealed class PostgresProjectionCheckpointStore : IProjectionCheckpointStore
{
    private readonly NpgsqlDataSource _dataSource;

    public PostgresProjectionCheckpointStore(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<long> GetLastPositionAsync(string projectionName)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = @"
            select ""LastPosition""
            from projections.projection_checkpoints
            where ""ProjectionName"" = @name";

        cmd.Parameters.AddWithValue("name", projectionName);

        var result = await cmd.ExecuteScalarAsync();

        return result == null ? 0 : (long)result;
    }

    public async Task SavePositionAsync(string projectionName, long position)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = @"
            insert into projections.projection_checkpoints
                (""ProjectionName"", ""LastPosition"", ""UpdatedUtc"")
            values
                (@name, @pos, now())
            on conflict (""ProjectionName"")
            do update set
                ""LastPosition"" = excluded.""LastPosition"",
                ""UpdatedUtc"" = now();";

        cmd.Parameters.AddWithValue("name", projectionName);
        cmd.Parameters.AddWithValue("pos", position);

        await cmd.ExecuteNonQueryAsync();
    }
}
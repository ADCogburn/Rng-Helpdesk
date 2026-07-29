using Microsoft.EntityFrameworkCore;
using Npgsql;
using RngHelpdesk.Infrastructure.Persistence.Contexts;
using Testcontainers.PostgreSql;

namespace RngHelpdesk.Infrastructure.Tests;

public sealed class MigrationFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16")
        .Build();

    public string ConnectionString => _postgres.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new AppDbContext(options);
    }

    public async Task DisposeAsync() => await _postgres.DisposeAsync();
}

public sealed class MigrationTests : IClassFixture<MigrationFixture>
{
    private readonly MigrationFixture _fixture;

    public MigrationTests(MigrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Migration_CreatesExpectedSchemasAndTables()
    {
        var schemas = await QueryAsync(
            "SELECT schema_name FROM information_schema.schemata WHERE schema_name IN ('identity','eventstore','projections','points')");

        Assert.Equal(
            new[] { "eventstore", "identity", "points", "projections" },
            schemas.OrderBy(s => s).ToArray());

        var tables = await QueryAsync(
            """
            SELECT table_schema || '.' || table_name
            FROM information_schema.tables
            WHERE table_schema IN ('identity', 'eventstore', 'projections', 'points')
            """);

        Assert.Equal(
            new[]
            {
                "eventstore.event_store",
                "eventstore.event_streams",
                "identity.auth_users",
                "points.rank_thresholds",
                "projections.projection_checkpoints"
            },
            tables.OrderBy(t => t).ToArray());
    }

    [Fact]
    public async Task Migration_UsesBigintForStreamId()
    {
        var dataTypes = await QueryAsync(
            """
            SELECT data_type
            FROM information_schema.columns
            WHERE table_schema = 'eventstore'
              AND table_name IN ('event_store', 'event_streams')
              AND column_name = 'StreamId'
            """);

        Assert.Equal(2, dataTypes.Count);
        Assert.All(dataTypes, dataType => Assert.Equal("bigint", dataType));
    }

    [Fact]
    public async Task Migration_SeedsCanonicalRankThresholds()
    {
        await using var context = _fixture.CreateContext();

        var thresholds = await context.Set<RankThresholdRow>()
            .OrderBy(x => x.SortOrder)
            .ToListAsync();

        Assert.Equal(14, thresholds.Count);
        Assert.Contains(thresholds, t => t.Rank == "Bronze" && t.PointsRequired == 0);
        Assert.Contains(thresholds, t => t.Rank == "Iron" && t.PointsRequired == 10);
        Assert.Contains(thresholds, t => t.Rank == "Zenyte" && t.PointsRequired == 5000);
    }

    private async Task<List<string>> QueryAsync(string sql)
    {
        await using var connection = new NpgsqlConnection(_fixture.ConnectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        var results = new List<string>();
        while (await reader.ReadAsync())
        {
            results.Add(reader.GetString(0));
        }

        return results;
    }
}

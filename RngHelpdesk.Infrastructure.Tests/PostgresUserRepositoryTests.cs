using Npgsql;
using RngHelpdesk.Domain.Common;
using RngHelpdesk.Domain.Users;
using RngHelpdesk.Domain.Users.Events;
using RngHelpdesk.Infrastructure.Common;
using RngHelpdesk.Infrastructure.Persistence.EventStore;
using RngHelpdesk.Infrastructure.Users;

namespace RngHelpdesk.Infrastructure.Tests;

public sealed class PostgresUserRepositoryTests : IClassFixture<MigrationFixture>
{
    private readonly MigrationFixture _fixture;

    public PostgresUserRepositoryTests(MigrationFixture fixture)
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
    public async Task GetByIdAsync_AfterAppendingEventsDirectlyViaEventStore_RehydratesMatchingUser()
    {
        var (dataSource, store, repo) = CreateRepo();
        var userId = NewStreamId();

        var discordAccount = new DiscordAccount(userId, "SomeDiscordUser");

        var createdEvent = UserCreatedEvent.Create(
            actingUserId: 1,
            discordAccount: discordAccount,
            runescapeAccounts: [new RunescapeAccount("SomeRsAccount")]);

        var linkedEvent = RunescapeAccountLinkedEvent.Create(
            actingUserId: 1,
            userId: userId,
            username: "SecondRsAccount");

        await store.AppendAsync("User", userId, expectedVersion: 0, [createdEvent], new EventStoreMetadata());
        await store.AppendAsync("User", userId, expectedVersion: 1, [linkedEvent], new EventStoreMetadata());

        var user = await repo.GetByIdAsync(userId);

        Assert.Equal(userId, user.Id);
        Assert.True(user.IsActive);
        Assert.Equal(2, user.Version);
        Assert.Equal("SomeDiscordUser", user.DiscordAccount.Username);
        Assert.Equal(
            new[] { "SecondRsAccount", "SomeRsAccount" },
            user.RunescapeAccounts.Select(a => a.Username).OrderBy(x => x));

        await dataSource.DisposeAsync();
    }

    [Fact]
    public async Task SaveAsync_ThenGetByIdAsync_RoundTripsAggregateThroughPostgres()
    {
        var (dataSource, _, repo) = CreateRepo();
        var userId = NewStreamId();

        var user = User.Create(
            actingUserId: 1,
            discordAccount: new DiscordAccount(userId, "RoundTripUser"),
            runescapeAccounts: []);

        user.AddRunescapeAccount(actingUserId: 1, username: "RoundTripRsAccount");

        var savedEvents = await repo.SaveAsync(user);

        Assert.Equal(2, savedEvents.Count);
        Assert.Empty(user.UncommittedDomainEvents);

        var reloaded = await repo.GetByIdAsync(userId);

        Assert.Equal(userId, reloaded.Id);
        Assert.Equal("RoundTripUser", reloaded.DiscordAccount.Username);
        Assert.Equal(["RoundTripRsAccount"], reloaded.RunescapeAccounts.Select(a => a.Username));

        await dataSource.DisposeAsync();
    }

    [Fact]
    public async Task GetByIdAsync_WhenStreamDoesNotExist_ThrowsAggregateNotFoundException()
    {
        var (dataSource, _, repo) = CreateRepo();

        await Assert.ThrowsAsync<AggregateNotFoundException>(() => repo.GetByIdAsync(NewStreamId()));

        await dataSource.DisposeAsync();
    }

    [Fact]
    public async Task ExistsAsync_And_UserExistsWithDiscordIdAsync_ReflectWhetherStreamHasBeenCreated()
    {
        var (dataSource, _, repo) = CreateRepo();
        var userId = NewStreamId();

        Assert.False(await repo.ExistsAsync(userId));
        Assert.False(await repo.UserExistsWithDiscordIdAsync(userId));

        var user = User.Create(
            actingUserId: 1,
            discordAccount: new DiscordAccount(userId, "ExistsCheckUser"),
            runescapeAccounts: []);

        await repo.SaveAsync(user);

        Assert.True(await repo.ExistsAsync(userId));
        Assert.True(await repo.UserExistsWithDiscordIdAsync(userId));

        await dataSource.DisposeAsync();
    }

    [Fact]
    public async Task UserExistsWithDiscordUsernameAsync_MatchesCaseInsensitivelyOnlyAfterCreation()
    {
        var (dataSource, _, repo) = CreateRepo();
        var userId = NewStreamId();
        var username = $"UsernameCheck-{userId}";

        Assert.False(await repo.UserExistsWithDiscordUsernameAsync(username));

        var user = User.Create(
            actingUserId: 1,
            discordAccount: new DiscordAccount(userId, username),
            runescapeAccounts: []);

        await repo.SaveAsync(user);

        Assert.True(await repo.UserExistsWithDiscordUsernameAsync(username.ToUpperInvariant()));
        Assert.False(await repo.UserExistsWithDiscordUsernameAsync($"not-{username}"));

        await dataSource.DisposeAsync();
    }
}

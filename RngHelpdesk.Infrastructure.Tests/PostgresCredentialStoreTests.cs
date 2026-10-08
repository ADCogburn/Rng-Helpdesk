using RngHelpdesk.Infrastructure.Security;

namespace RngHelpdesk.Infrastructure.Tests;

public sealed class PostgresCredentialStoreTests : IClassFixture<MigrationFixture>
{
    private readonly MigrationFixture _fixture;

    public PostgresCredentialStoreTests(MigrationFixture fixture)
    {
        _fixture = fixture;
    }

    private static ulong NewUserId() => (ulong)Random.Shared.NextInt64(1, long.MaxValue);

    [Fact]
    public async Task CreateValidateChangePassword_RoundTripsThroughARealPostgresInstance()
    {
        var userId = NewUserId();
        string username;
        string temporaryPassword;

        await using (var db = _fixture.CreateContext())
        {
            (username, temporaryPassword) = await new PostgresCredentialStore(db)
                .CreateTemporaryCredentialsAsync(userId, "RoundTripUser");
        }

        // Each step below re-opens a fresh AppDbContext (a fresh Postgres connection), proving
        // state actually persisted rather than just living in one EF change tracker's cache.
        await using (var db = _fixture.CreateContext())
        {
            var authenticated = await new PostgresCredentialStore(db)
                .ValidateCredentialsAsync(username, temporaryPassword);

            Assert.NotNull(authenticated);
            Assert.Equal(userId, authenticated!.UserId);
            Assert.Equal(username, authenticated.Username);
            Assert.True(authenticated.MustChangePassword);
        }

        await using (var db = _fixture.CreateContext())
        {
            await new PostgresCredentialStore(db).ChangePasswordAsync(userId, "a-new-password");
        }

        await using (var db = _fixture.CreateContext())
        {
            var store = new PostgresCredentialStore(db);

            Assert.Null(await store.ValidateCredentialsAsync(username, temporaryPassword));

            var afterChange = await store.ValidateCredentialsAsync(username, "a-new-password");
            Assert.NotNull(afterChange);
            Assert.Equal(userId, afterChange!.UserId);
            Assert.False(afterChange.MustChangePassword);
        }
    }

    [Fact]
    public async Task SeedCredentialsAsync_PersistsCredentialsWithoutMustChangePassword()
    {
        await using var db = _fixture.CreateContext();
        var store = new PostgresCredentialStore(db);
        var userId = NewUserId();

        await store.SeedCredentialsAsync(userId, $"seeded-{userId}", "seed-password");

        var authenticated = await store.ValidateCredentialsAsync($"seeded-{userId}", "seed-password");
        Assert.NotNull(authenticated);
        Assert.Equal(userId, authenticated!.UserId);
        Assert.False(authenticated.MustChangePassword);
    }

    [Fact]
    public async Task SeedCredentialsAsync_CalledAgainForSameUser_OverwritesThePriorCredential()
    {
        await using var db = _fixture.CreateContext();
        var store = new PostgresCredentialStore(db);
        var userId = NewUserId();

        await store.SeedCredentialsAsync(userId, $"seeded-{userId}", "first-password");
        await store.SeedCredentialsAsync(userId, $"seeded-{userId}", "second-password");

        Assert.Null(await store.ValidateCredentialsAsync($"seeded-{userId}", "first-password"));
        Assert.NotNull(await store.ValidateCredentialsAsync($"seeded-{userId}", "second-password"));
    }

    [Fact]
    public async Task ValidateCredentialsAsync_WrongPassword_ReturnsNull()
    {
        await using var db = _fixture.CreateContext();
        var store = new PostgresCredentialStore(db);
        var userId = NewUserId();

        await store.SeedCredentialsAsync(userId, $"wrongpw-{userId}", "correct-password");

        Assert.Null(await store.ValidateCredentialsAsync($"wrongpw-{userId}", "incorrect-password"));
    }

    [Fact]
    public async Task ValidateCredentialsAsync_UnknownUsername_ReturnsNull()
    {
        await using var db = _fixture.CreateContext();
        var store = new PostgresCredentialStore(db);

        Assert.Null(await store.ValidateCredentialsAsync("no-such-user", "whatever"));
    }

    [Fact]
    public async Task CreateTemporaryCredentialsAsync_PreferredUsernameAlreadyTaken_GeneratesASuffixedUsername()
    {
        await using var db = _fixture.CreateContext();
        var store = new PostgresCredentialStore(db);

        var (firstUsername, _) = await store.CreateTemporaryCredentialsAsync(NewUserId(), "duplicate-name");
        var (secondUsername, _) = await store.CreateTemporaryCredentialsAsync(NewUserId(), "duplicate-name");

        Assert.Equal("duplicate-name", firstUsername);
        Assert.Equal("duplicate-name_2", secondUsername);
    }

    [Fact]
    public async Task ChangePasswordAsync_UnknownUser_Throws()
    {
        await using var db = _fixture.CreateContext();
        var store = new PostgresCredentialStore(db);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => store.ChangePasswordAsync(NewUserId(), "new-password"));
    }
}

using Microsoft.EntityFrameworkCore;
using RngHelpdesk.Infrastructure.Persistence.Contexts;

namespace RngHelpdesk.Infrastructure.Security;

public sealed class PostgresCredentialStore : ICredentialStore
{
    private readonly AppDbContext _db;

    public PostgresCredentialStore(AppDbContext db)
    {
        _db = db;
    }

    public async Task<(string Username, string TemporaryPassword)> CreateTemporaryCredentialsAsync(
        ulong userId,
        string preferredUsername,
        CancellationToken ct = default)
    {
        var username = await GenerateUniqueUsernameAsync(preferredUsername, ct);
        var password = CredentialGenerator.GenerateTemporaryPassword();

        _db.Set<AuthUserRow>().Add(new AuthUserRow
        {
            Username = username,
            UserId = (long)userId,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            MustChangePassword = true
        });

        await _db.SaveChangesAsync(ct);

        return (username, password);
    }

    public async Task SeedCredentialsAsync(
        ulong userId,
        string username,
        string password,
        CancellationToken ct = default)
    {
        var row = await _db.Set<AuthUserRow>().SingleOrDefaultAsync(x => x.UserId == (long)userId, ct);

        if (row is null)
        {
            _db.Set<AuthUserRow>().Add(new AuthUserRow
            {
                Username = username,
                UserId = (long)userId,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                MustChangePassword = false
            });
        }
        else
        {
            row.Username = username;
            row.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            row.MustChangePassword = false;
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task<AuthenticatedUser?> ValidateCredentialsAsync(
        string username,
        string password,
        CancellationToken ct = default)
    {
        // Case-insensitive lookup to match InMemoryCredentialStore's OrdinalIgnoreCase username index.
        var row = await _db.Set<AuthUserRow>().SingleOrDefaultAsync(x => x.Username.ToLower() == username.ToLower(), ct);

        if (row is null || !BCrypt.Net.BCrypt.Verify(password, row.PasswordHash))
            return null;

        return new AuthenticatedUser(
            UserId: (ulong)row.UserId,
            Username: row.Username,
            MustChangePassword: row.MustChangePassword);
    }

    public async Task ChangePasswordAsync(
        ulong userId,
        string newPassword,
        CancellationToken ct = default)
    {
        var row = await _db.Set<AuthUserRow>().SingleOrDefaultAsync(x => x.UserId == (long)userId, ct)
            ?? throw new InvalidOperationException("User not found.");

        row.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        row.MustChangePassword = false;

        await _db.SaveChangesAsync(ct);
    }

    private async Task<string> GenerateUniqueUsernameAsync(string preferredUsername, CancellationToken ct)
    {
        preferredUsername = CredentialGenerator.NormalizeUsername(preferredUsername);

        if (!await _db.Set<AuthUserRow>().AnyAsync(x => x.Username.ToLower() == preferredUsername, ct))
            return preferredUsername;

        var suffix = 2;

        while (await _db.Set<AuthUserRow>().AnyAsync(x => x.Username.ToLower() == $"{preferredUsername}_{suffix}", ct))
        {
            suffix++;
        }

        return $"{preferredUsername}_{suffix}";
    }
}

using System.Security.Cryptography;

namespace RngHelpdesk.Infrastructure.Security;

public sealed class InMemoryCredentialStore : ICredentialStore
{
    private readonly Dictionary<string, ulong> _usernameIndex = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<ulong, CredentialRecord> _credentials = new();

    public Task<(string Username, string TemporaryPassword)> CreateTemporaryCredentialsAsync(
        ulong userId,
        string preferredUsername,
        CancellationToken ct = default)
    {
        var username = GenerateUniqueUsername(preferredUsername);
        var password = GenerateTemporaryPassword();

        _credentials[userId] = new CredentialRecord(
            UserId: userId,
            Username: username,
            PasswordHash: BCrypt.Net.BCrypt.HashPassword(password),
            MustChangePassword: true);

        _usernameIndex[username] = userId;

        return Task.FromResult((username, password));
    }

    public Task SeedCredentialsAsync(
        ulong userId,
        string username,
        string password,
        CancellationToken ct = default)
    {
        _credentials[userId] = new CredentialRecord(
            UserId: userId,
            Username: username,
            PasswordHash: BCrypt.Net.BCrypt.HashPassword(password),
            MustChangePassword: false);

        _usernameIndex[username] = userId;

        return Task.CompletedTask;
    }

    public Task<AuthenticatedUser?> ValidateCredentialsAsync(
        string username,
        string password,
        CancellationToken ct = default)
    {
        if (!_usernameIndex.TryGetValue(username, out var userId))
            return Task.FromResult<AuthenticatedUser?>(null);

        if (!_credentials.TryGetValue(userId, out var user))
            return Task.FromResult<AuthenticatedUser?>(null);

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return Task.FromResult<AuthenticatedUser?>(null);

        return Task.FromResult<AuthenticatedUser?>(new AuthenticatedUser(
            UserId: user.UserId,
            Username: user.Username,
            MustChangePassword: user.MustChangePassword));
    }

    public Task ChangePasswordAsync(
        ulong userId,
        string newPassword,
        CancellationToken ct = default)
    {
        if (!_credentials.TryGetValue(userId, out var user))
            throw new InvalidOperationException("User not found.");

        _credentials[userId] = user with
        {
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword),
            MustChangePassword = false
        };

        return Task.CompletedTask;
    }

    private static string GenerateTemporaryPassword() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(12));

    private string GenerateUniqueUsername(string preferredUsername)
    {
        if (string.IsNullOrWhiteSpace(preferredUsername))
            preferredUsername = $"user{Random.Shared.Next(1000, 9999)}";

        preferredUsername = preferredUsername
            .Trim()
            .Replace(" ", "")
            .ToLowerInvariant();

        if (!_usernameIndex.ContainsKey(preferredUsername))
            return preferredUsername;

        var suffix = 2;

        while (_usernameIndex.ContainsKey($"{preferredUsername}_{suffix}"))
        {
            suffix++;
        }

        return $"{preferredUsername}_{suffix}";
    }
}

using System.Security.Cryptography;

namespace RngHelpdesk.Infrastructure.Security;

public sealed class InMemoryCredentialStore : ICredentialStore
{
    private readonly Dictionary<string, ulong> _usernameIndex = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<ulong, CredentialRecord> _credentials = new();

    public (string Username, string TemporaryPassword) CreateTemporaryCredentials(
        ulong userId,
        string preferredUsername)
    {
        var username = GenerateUniqueUsername(preferredUsername);
        var password = GenerateTemporaryPassword();

        _credentials[userId] = new CredentialRecord(
            UserId: userId,
            Username: username,
            PasswordHash: BCrypt.Net.BCrypt.HashPassword(password),
            MustChangePassword: true);

        _usernameIndex[username] = userId;

        return (username, password);
    }

    public void SeedCredentials(
        ulong userId,
        string username,
        string password)
    {
        _credentials[userId] = new CredentialRecord(
            UserId: userId,
            Username: username,
            PasswordHash: BCrypt.Net.BCrypt.HashPassword(password),
            MustChangePassword: false);

        _usernameIndex[username] = userId;
    }

    public AuthenticatedUser? ValidateCredentials(
        string username,
        string password)
    {
        if (!_usernameIndex.TryGetValue(username, out var userId))
            return null;

        if (!_credentials.TryGetValue(userId, out var user))
            return null;

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        return new AuthenticatedUser(
            UserId: user.UserId,
            Username: user.Username,
            MustChangePassword: user.MustChangePassword);
    }

    public void ChangePassword(
        ulong userId,
        string newPassword)
    {
        if (!_credentials.TryGetValue(userId, out var user))
            throw new InvalidOperationException("User not found.");

        _credentials[userId] = user with
        {
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword),
            MustChangePassword = false
        };
    }

    public void ChangeUsername(
        ulong userId,
        string newUsername)
    {
        if (!_credentials.TryGetValue(userId, out var user))
            throw new InvalidOperationException("User not found.");

        _credentials[userId] = user with
        {
            Username = newUsername
        };
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

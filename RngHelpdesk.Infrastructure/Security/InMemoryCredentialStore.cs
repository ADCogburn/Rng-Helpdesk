using System.Security.Cryptography;

namespace RngHelpdesk.Infrastructure.Security;

public sealed class InMemoryCredentialStore : ICredentialStore
{
    private readonly Dictionary<string, CredentialRecord> _users = new(StringComparer.OrdinalIgnoreCase);

    public (string Username, string TemporaryPassword) CreateTemporaryCredentials(
        ulong userId,
        string preferredUsername)
    {
        var username = GenerateUniqueUsername(preferredUsername);
        var password = GenerateTemporaryPassword();

        _users[username] = new CredentialRecord
        (
            UserId: userId,
            Username: username,
            PasswordHash: BCrypt.Net.BCrypt.HashPassword(password),
            MustChangePassword: true
        );

        return (username, password);
    }

    public AuthenticatedUser? ValidateCredentials(string username, string password)
    {
        if (!_users.TryGetValue(username, out var user))
            return null;

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        return new AuthenticatedUser(UserId: user.UserId, Username: user.Username, MustChangePassword: user.MustChangePassword);
    }

    public void ChangePassword(string username, string newPassword)
    {
        if (!_users.TryGetValue(username, out var user))
            throw new InvalidOperationException("User not found.");

        _users[username] = user with
        {
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword),
            MustChangePassword = false
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

        if (!_users.ContainsKey(preferredUsername))
            return preferredUsername;

        var suffix = 2;

        while (_users.ContainsKey($"{preferredUsername}_{suffix}"))
        {
            suffix++;
        }

        return $"{preferredUsername}_{suffix}";
    }
}

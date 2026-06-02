using RngHelpdesk.Contracts.Security;
using System.Security.Cryptography;

namespace RngHelpdesk.Infrastructure.Security;

public sealed class InMemoryAuthStore : IAuthStore
{
    private readonly Dictionary<string, UserAuthDetails> _users = new(StringComparer.OrdinalIgnoreCase);

    public (string Username, string TemporaryPassword) CreateTemporaryCredentials(
        int userId,
        string preferredUsername,
        AppRole role)
    {
        var username = MakeUniqueUsername(preferredUsername);
        var password = GeneratePassword();

        _users[username] = new UserAuthDetails
        {
            UserId = userId,
            Username = username,
            Role = role,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            MustChangePassword = true
        };

        return (username, password);
    }

    public AuthenticatedUser? ValidateCredentials(string username, string password)
    {
        if (!_users.TryGetValue(username, out var user))
            return null;

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        return new AuthenticatedUser(user.UserId, user.Role);
    }

    public void ChangePassword(string username, string newPassword)
    {
        if (!_users.TryGetValue(username, out var user))
            throw new InvalidOperationException("User not found.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.MustChangePassword = false;
    }

    public void ChangeRole(int userId, AppRole newRole)
    {
        var user = _users.Values.FirstOrDefault(x => x.UserId == userId);

        if (user is null)
            throw new InvalidOperationException("User auth record not found.");

        user.Role = newRole;
    }

    private static string GeneratePassword() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(12));

    private string MakeUniqueUsername(string baseUsername)
    {
        if (!_users.ContainsKey(baseUsername))
            return baseUsername;

        return $"{baseUsername}_{Random.Shared.Next(1000, 9999)}";
    }
}

using System.Security.Cryptography;

namespace RngHelpdesk.Infrastructure.Security;

public sealed class InMemoryAuthStore : IAuthStore
{
    private readonly Dictionary<string, AuthUser> _users = new(StringComparer.OrdinalIgnoreCase);

    public (string Username, string TemporaryPassword) CreateTemporaryCredentials(
        int userId,
        Guid actorId,
        string preferredUsername)
    {
        var username = MakeUniqueUsername(preferredUsername);
        var password = GeneratePassword();

        _users[username] = new AuthUser
        {
            UserId = userId,
            ActorId = actorId,
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            MustChangePassword = true
        };

        return (username, password);
    }

    public Guid? ValidateCredentials(string username, string password)
    {
        if (!_users.TryGetValue(username, out var user))
            return null;

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        return user.ActorId;
    }

    public void ChangePassword(string username, string newPassword)
    {
        if (!_users.TryGetValue(username, out var user))
            throw new InvalidOperationException("User not found.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.MustChangePassword = false;
    }

    private static string GeneratePassword() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(12));

    private string MakeUniqueUsername(string baseUsername)
    {
        if (!_users.ContainsKey(baseUsername))
            return baseUsername;

        return $"{baseUsername}_{Random.Shared.Next(1000, 9999)}";
    }

    public void SeedUser(
         int userId,
         Guid actorId,
         string username,
         string password,
         bool mustChangePassword = false)
    {
        _users[username] = new AuthUser
        {
            UserId = userId,
            ActorId = actorId,
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            MustChangePassword = mustChangePassword
        };
    }
}

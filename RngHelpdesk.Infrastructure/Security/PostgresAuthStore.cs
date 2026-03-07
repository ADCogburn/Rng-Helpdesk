using RngHelpdesk.Infrastructure.Persistence.Contexts;
using System.Security.Cryptography;

namespace RngHelpdesk.Infrastructure.Security;

public sealed class PostgresAuthStore : IAuthStore
{
    private readonly AppDbContext _db;

    public PostgresAuthStore(AppDbContext db)
    {
        _db = db;
    }

    public (string Username, string TemporaryPassword) CreateTemporaryCredentials(
        int userId,
        Guid actorId,
        string preferredUsername)
    {
        var username = MakeUniqueUsername(preferredUsername);
        var password = GeneratePassword();

        _db.Add(new AuthUserRow
        {
            Username = username,
            UserId = userId,
            ActorId = actorId,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            MustChangePassword = true
        });

        _db.SaveChanges();

        return (username, password);
    }

    public Guid? ValidateCredentials(string username, string password)
    {
        var user = _db.Set<AuthUserRow>().SingleOrDefault(x => x.Username == username);
        if (user == null) return null;

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        return user.ActorId;
    }

    public void ChangePassword(string username, string newPassword)
    {
        var user = _db.Set<AuthUserRow>().Single(x => x.Username == username);

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.MustChangePassword = false;

        _db.SaveChanges();
    }

    private static string GeneratePassword()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(12));

    private string MakeUniqueUsername(string baseUsername)
    {
        if (!_db.Set<AuthUserRow>().Any(x => x.Username == baseUsername))
            return baseUsername;

        return $"{baseUsername}_{Random.Shared.Next(1000, 9999)}";
    }

    public void SeedUser(int userId, Guid actorId, string username, string password, bool mustChangePassword = false)
    {
        _db.Add(new AuthUserRow
        {
            Username = username,
            UserId = userId,
            ActorId = actorId,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            MustChangePassword = mustChangePassword
        });

        _db.SaveChanges();
    }
}


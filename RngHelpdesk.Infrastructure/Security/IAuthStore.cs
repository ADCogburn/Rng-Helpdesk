namespace RngHelpdesk.Infrastructure.Security;

public interface IAuthStore
{
    (string Username, string TemporaryPassword) CreateTemporaryCredentials(
        int userId,
        Guid actorId,
        string preferredUsername);

    Guid? ValidateCredentials(string username, string password);

    void ChangePassword(string username, string newPassword);

    void SeedUser(int userId, Guid actorId, string username, string password, bool mustChangePassword = false);
}

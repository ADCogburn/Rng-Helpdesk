using RngHelpdesk.Contracts.Security;

namespace RngHelpdesk.Infrastructure.Security;

public interface IAuthStore
{
    (string Username, string TemporaryPassword) CreateTemporaryCredentials(
        int userId,
        string preferredUsername,
        AppRole role);

    AuthenticatedUser? ValidateCredentials(string username, string password);

    void ChangePassword(string username, string newPassword);

    void ChangeRole(int userId, AppRole newRole);
}

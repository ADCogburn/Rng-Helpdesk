namespace RngHelpdesk.Infrastructure.Security;

public interface ICredentialStore
{
    (string Username, string TemporaryPassword) CreateTemporaryCredentials(ulong userId, string preferredUsername);

    AuthenticatedUser? ValidateCredentials(string username, string password);

    void ChangePassword(string username, string newPassword);
}

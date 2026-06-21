namespace RngHelpdesk.Infrastructure.Security;

public interface ICredentialStore
{
    (string Username, string TemporaryPassword) CreateTemporaryCredentials(
        ulong userId,
        string preferredUsername);

    void SeedCredentials(
        ulong userId,
        string username,
        string password);

    AuthenticatedUser? ValidateCredentials(
        string username,
        string password);

    void ChangePassword(
        ulong userId,
        string newPassword);

    void ChangeUsername(
        ulong userId,
        string newUsername);
}

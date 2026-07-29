namespace RngHelpdesk.Infrastructure.Security;

public interface ICredentialStore
{
    Task<(string Username, string TemporaryPassword)> CreateTemporaryCredentialsAsync(
        ulong userId,
        string preferredUsername,
        CancellationToken ct = default);

    Task SeedCredentialsAsync(
        ulong userId,
        string username,
        string password,
        CancellationToken ct = default);

    Task<AuthenticatedUser?> ValidateCredentialsAsync(
        string username,
        string password,
        CancellationToken ct = default);

    Task ChangePasswordAsync(
        ulong userId,
        string newPassword,
        CancellationToken ct = default);
}

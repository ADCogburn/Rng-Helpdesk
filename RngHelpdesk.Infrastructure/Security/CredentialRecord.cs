namespace RngHelpdesk.Infrastructure.Security;

internal sealed record CredentialRecord(
    ulong UserId,
    string Username,
    string PasswordHash,
    bool MustChangePassword);
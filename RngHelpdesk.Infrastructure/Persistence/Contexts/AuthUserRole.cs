namespace RngHelpdesk.Infrastructure.Persistence.Contexts;

internal sealed class AuthUserRow
{
    public string Username { get; set; } = default!;
    public long UserId { get; set; }

    public string PasswordHash { get; set; } = default!;
    public bool MustChangePassword { get; set; }
}
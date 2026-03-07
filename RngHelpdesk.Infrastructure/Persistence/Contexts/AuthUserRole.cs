namespace RngHelpdesk.Infrastructure.Persistence.Contexts;

internal sealed class AuthUserRow
{
    public string Username { get; set; } = default!;
    public int UserId { get; set; }
    public Guid ActorId { get; set; }

    public string PasswordHash { get; set; } = default!;
    public bool MustChangePassword { get; set; }
}
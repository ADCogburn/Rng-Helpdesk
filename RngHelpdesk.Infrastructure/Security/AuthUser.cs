namespace RngHelpdesk.Infrastructure.Security;

internal sealed class AuthUser
{
    public int UserId { get; init; }
    public Guid ActorId { get; init; }

    public string Username { get; init; } = default!;
    public string PasswordHash { get; set; } = default!;

    public bool MustChangePassword { get; set; }
}
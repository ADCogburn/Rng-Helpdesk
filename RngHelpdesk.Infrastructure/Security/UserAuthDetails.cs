using RngHelpdesk.Contracts.Security;

namespace RngHelpdesk.Infrastructure.Security;

internal sealed class UserAuthDetails
{
    public int UserId { get; init; }
    public AppRole Role { get; set; }

    public string Username { get; init; } = default!;
    public string PasswordHash { get; set; } = default!;

    public bool MustChangePassword { get; set; }
}
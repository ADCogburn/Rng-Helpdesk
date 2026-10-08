namespace RngHelpdesk.Api.DTOs;

public sealed class LoginResponse
{
    public required string Token { get; init; }
    public bool MustChangePassword { get; init; }
}

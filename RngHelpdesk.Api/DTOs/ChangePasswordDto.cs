namespace RngHelpdesk.Api.DTOs;

public sealed class ChangePasswordDto
{
    public string CurrentPassword { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}

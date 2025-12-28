namespace RngHelpdesk.Api.DTOs;

public sealed class RemovePointsDto
{
    public int Points { get; init; }
    public string Reason { get; init; } = string.Empty;
}

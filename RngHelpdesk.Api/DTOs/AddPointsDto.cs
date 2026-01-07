namespace RngHelpdesk.Api.DTOs;

public sealed class AddPointsDto
{
    public int Points { get; set; }
    public string Reason { get; set; } = string.Empty;
}

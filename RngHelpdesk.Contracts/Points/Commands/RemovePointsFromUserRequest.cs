namespace RngHelpdesk.Contracts.Points.Commands;

public sealed class RemovePointsFromUserRequest
{
    public int UserId { get; set; }
    public int Points { get; set; }
    public string Reason { get; set; } = string.Empty;
}
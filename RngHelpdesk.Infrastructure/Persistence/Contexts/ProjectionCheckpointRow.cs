namespace RngHelpdesk.Infrastructure.Persistence.Contexts;

internal sealed class ProjectionCheckpointRow
{
    public string ProjectionName { get; set; } = default!;
    public long LastPosition { get; set; }
    public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;
}

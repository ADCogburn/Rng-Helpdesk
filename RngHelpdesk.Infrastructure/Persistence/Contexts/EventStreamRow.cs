namespace RngHelpdesk.Infrastructure.Persistence.Contexts;

internal sealed class EventStreamRow
{
    public string StreamType { get; set; } = default!;
    public int StreamId { get; set; }

    public int CurrentVersion { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;
}

namespace RngHelpdesk.Infrastructure.Persistence.Contexts;

internal sealed class EventRecordRow
{
    public long GlobalPosition { get; set; }

    public string StreamType { get; set; } = default!;
    public long StreamId { get; set; }
    public int StreamVersion { get; set; }

    public string EventType { get; set; } = default!;
    public int EventSchemaVer { get; set; }

    public DateTime OccurredUtc { get; set; }
    public DateTime RecordedUtc { get; set; }

    public string Payload { get; set; } = default!;
    public string Metadata { get; set; } = default!;
}

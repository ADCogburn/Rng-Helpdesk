namespace RngHelpdesk.Infrastructure.Common;

public sealed class AggregateNotFoundException : Exception
{
    public AggregateNotFoundException(string aggregateName, ulong id) : base($"{aggregateName} not found.")
    {
        AggregateName = aggregateName;
        Id = id;
    }

    public string AggregateName { get; }
    public ulong Id { get; }
}

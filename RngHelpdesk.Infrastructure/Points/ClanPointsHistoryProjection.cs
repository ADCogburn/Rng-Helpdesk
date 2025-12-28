using RngHelpdesk.Operations.Common;

public sealed class ClanPointsHistoryProjection : IDomainEventHandler<ClanPointsChangedEvent>
{
    public Task HandleAsync(ClanPointsChangedEvent e)
    {
        // TODO: this should create a ClanPointsHistory record in the database, which can later be queried for audit purposes.
        return Task.CompletedTask;
    }
}
using RngHelpdesk.Contracts.Security;

namespace RngHelpdesk.Infrastructure.Persistence.EventStore;

public interface IEventStoreMetadataProvider
{
    EventStoreMetadata GetMetadata();
}

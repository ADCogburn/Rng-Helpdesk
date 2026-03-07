using RngHelpdesk.Contracts.Security;

namespace RngHelpdesk.Infrastructure.Persistence.EventStore;

public sealed class RequestContextEventStoreMetadataProvider : IEventStoreMetadataProvider
{
    private readonly IRequestContextAccessor _contextAccessor;

    public RequestContextEventStoreMetadataProvider(IRequestContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public EventStoreMetadata GetMetadata()
    {
        var ctx = _contextAccessor.Context;
        if (!ctx.IsAuthenticated)
            return new EventStoreMetadata(null, null, null, null);

        return new EventStoreMetadata(
            ctx.ActorId,
            ctx.ActorType.ToString(),
            null,
            null);
    }
}

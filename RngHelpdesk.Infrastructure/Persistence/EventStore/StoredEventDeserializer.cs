using RngHelpdesk.Domain.Common;
using System.Text.Json;

namespace RngHelpdesk.Infrastructure.Persistence.EventStore;

public static class StoredEventDeserializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static IDomainEvent Deserialize(StoredEvent stored, Type clrType)
    {
        var obj = JsonSerializer.Deserialize(stored.PayloadJson, clrType, Options);

        if (obj is not IDomainEvent domainEvent)
            throw new InvalidOperationException($"Deserialized event was not IDomainEvent. EventType={stored.EventType}");

        return domainEvent;
    }
}

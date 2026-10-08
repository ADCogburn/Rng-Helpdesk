using RngHelpdesk.Domain.Common;
using System.Text.Json;

namespace RngHelpdesk.Infrastructure.Persistence.EventStore;

public static class StoredEventDeserializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static IEvent Deserialize(StoredEvent stored, Type clrType)
    {
        var obj = JsonSerializer.Deserialize(stored.PayloadJson, clrType, Options);

        if (obj is not IEvent @event)
            throw new InvalidOperationException($"Deserialized event was not IEvent. EventType={stored.EventType}");

        return @event;
    }
}

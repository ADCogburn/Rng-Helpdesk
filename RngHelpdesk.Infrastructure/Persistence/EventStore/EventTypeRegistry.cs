using RngHelpdesk.Domain.Common;

namespace RngHelpdesk.Infrastructure.Persistence.EventStore;

/// <summary>
/// Helping class for registering a mapping of IDomainEvents to serialized (stringified) names, just in case the class names change down the line.
/// </summary>
public sealed class EventTypeRegistry
{
    private readonly Dictionary<string, Type> _byName = new();
    private readonly Dictionary<Type, string> _byType = new();

    public void Register<T>(string name) where T : IDomainEvent
    {
        _byName[name] = typeof(T);
        _byType[typeof(T)] = name;
    }

    public string GetName(Type type) => _byType[type];

    public Type GetType(string name) => _byName[name];
}
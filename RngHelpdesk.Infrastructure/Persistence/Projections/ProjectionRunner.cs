using RngHelpdesk.Domain.Common;
using RngHelpdesk.Infrastructure.Common; // where IProjectionHandler<T> lives
using RngHelpdesk.Infrastructure.Persistence.EventStore;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;

namespace RngHelpdesk.Infrastructure.Persistence.Projections;

public sealed class ProjectionRunner
{
    private readonly IEventStore _eventStore;
    private readonly EventTypeRegistry _registry;
    private readonly IProjectionCheckpointStore _checkpoints;
    private readonly IEnumerable<object> _projectionInstances;

    private readonly ConcurrentDictionary<Type, List<(object instance, MethodInfo method, string projectionName)>> _dispatchMap = new();

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ProjectionRunner(
        IEventStore eventStore,
        EventTypeRegistry registry,
        IProjectionCheckpointStore checkpoints,
        IEnumerable<object> projectionInstances)
    {
        _eventStore = eventStore;
        _registry = registry;
        _checkpoints = checkpoints;
        _projectionInstances = projectionInstances;

        BuildDispatchMap();
    }

    /// <summary>
    /// Replays all events after each projection's checkpoint and applies them in global order.
    /// Each projection keeps its own checkpoint (last processed global position).
    /// </summary>
    public async Task RunAsync(CancellationToken ct = default)
    {
        foreach (var projection in _projectionInstances)
        {
            var projectionName = projection.GetType().Name;

            // When projection state is empty (e.g. after app restart), replay from 0 to rebuild it.
            // Otherwise we'd have a checkpoint but no in-memory state, causing "User not found" etc.
            var lastPos = projection is IProjectionState state && state.IsEmpty
                ? 0L
                : await _checkpoints.GetLastPositionAsync(projectionName);

            // Load all events after that position (ordered)
            var storedEvents = await _eventStore.LoadFromPositionAsync(lastPos, ct);

            // Apply in order, checkpointing after each successful application
            foreach (var stored in storedEvents)
            {
                ct.ThrowIfCancellationRequested();

                var clrType = _registry.GetType(stored.EventType);
                if (!TryGetHandlersFor(projectionName, clrType, out var handlers))
                    continue;

                var domainEvent = DeserializeDomainEvent(stored, clrType);

                foreach (var (instance, method, _) in handlers)
                {
                    // Calls projection.Project((TEvent)domainEvent)
                    method.Invoke(instance, new object[] { domainEvent });
                }

                await _checkpoints.SavePositionAsync(projectionName, stored.GlobalPosition);
            }
        }
    }

    private IDomainEvent DeserializeDomainEvent(StoredEvent stored, Type clrType)
    {
        var obj = JsonSerializer.Deserialize(stored.PayloadJson, clrType, SerializerOptions);
        if (obj is not IDomainEvent ev)
            throw new InvalidOperationException($"Deserialized event was not IDomainEvent. EventType={stored.EventType}");

        return ev;
    }

    private bool TryGetHandlersFor(string projectionName, Type eventClrType,
        out List<(object instance, MethodInfo method, string projectionName)> handlers)
    {
        handlers = _dispatchMap.TryGetValue(eventClrType, out var list)
            ? list.Where(x => x.projectionName == projectionName).ToList()
            : new List<(object, MethodInfo, string)>();

        return handlers.Count > 0;
    }

    private void BuildDispatchMap()
    {
        foreach (var projection in _projectionInstances)
        {
            var projectionType = projection.GetType();
            var projectionName = projectionType.Name;

            // Find all interfaces like IProjectionHandler<TEvent>
            var handlerInterfaces = projectionType
                .GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IProjectionHandler<>))
                .ToList();

            foreach (var iface in handlerInterfaces)
            {
                var eventClrType = iface.GetGenericArguments()[0];

                var projectMethod = projectionType.GetMethod("Project", new[] { eventClrType });
                if (projectMethod is null)
                    throw new InvalidOperationException($"{projectionName} implements IProjectionHandler<{eventClrType.Name}> but has no Project({eventClrType.Name}) method.");

                _dispatchMap.AddOrUpdate(
                    eventClrType,
                    _ => new List<(object, MethodInfo, string)> { (projection, projectMethod, projectionName) },
                    (_, existing) =>
                    {
                        existing.Add((projection, projectMethod, projectionName));
                        return existing;
                    });
            }
        }
    }
}
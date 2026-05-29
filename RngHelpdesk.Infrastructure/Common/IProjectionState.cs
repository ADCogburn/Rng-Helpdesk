namespace RngHelpdesk.Infrastructure.Common;

/// <summary>
/// Optional interface for in-memory projections that lose state on app restart.
/// When IsEmpty is true, the ProjectionRunner will replay from position 0 instead of the checkpoint,
/// ensuring the projection is fully rebuilt before serving requests.
/// </summary>
public interface IProjectionState
{
    /// <summary>
    /// True when the projection has no data (e.g. after app restart).
    /// Causes a full replay from the event store.
    /// </summary>
    bool IsEmpty { get; }
}

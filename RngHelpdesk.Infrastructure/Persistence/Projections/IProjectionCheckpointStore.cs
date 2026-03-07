namespace RngHelpdesk.Infrastructure.Persistence.Projections;

/// <summary>
/// Stores and retrieves the last processed global event position for a projection.
/// </summary>
public interface IProjectionCheckpointStore
{
    Task<long> GetLastPositionAsync(string projectionName);
    Task SavePositionAsync(string projectionName, long position);
}
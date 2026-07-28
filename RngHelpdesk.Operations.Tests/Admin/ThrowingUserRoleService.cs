using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Infrastructure.Common;
using RngHelpdesk.Operations.Services;

namespace RngHelpdesk.Operations.Tests.Admin;

/// <summary>
/// IUserRoleService test double that always throws, standing in for an event-store
/// concurrency conflict (or any other failure) surfacing from ChangeRoleAsync.
/// </summary>
internal sealed class ThrowingUserRoleService : IUserRoleService
{
    private readonly Exception _exception;

    public ThrowingUserRoleService(Exception exception)
    {
        _exception = exception;
    }

    public Task<IReadOnlyCollection<IApplicationEvent>> ChangeRoleAsync(
        ulong actingUserId,
        ulong userId,
        AppRole oldRole,
        AppRole newRole)
        => throw _exception;
}

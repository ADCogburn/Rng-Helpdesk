using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Infrastructure.Common;

namespace RngHelpdesk.Operations.Services;

public interface IUserRoleService
{
    Task<IReadOnlyCollection<IApplicationEvent>> ChangeRoleAsync(
        ulong actingUserId,
        ulong userId,
        AppRole oldRole,
        AppRole newRole,
        CancellationToken ct = default);
}

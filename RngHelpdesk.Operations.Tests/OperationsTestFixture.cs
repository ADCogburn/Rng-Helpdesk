using RngHelpdesk.Contracts.Common.Ranks;
using RngHelpdesk.Domain.Users;
using RngHelpdesk.Infrastructure.Common;
using RngHelpdesk.Infrastructure.Persistence.EventStore;
using RngHelpdesk.Infrastructure.Persistence.Points;
using RngHelpdesk.Infrastructure.Points;
using RngHelpdesk.Infrastructure.Security;
using RngHelpdesk.Infrastructure.Users;
using RngHelpdesk.Infrastructure.Users.RunescapeAccount;
using RngHelpdesk.Operations.Services;

namespace RngHelpdesk.Operations.Tests;

/// <summary>
/// Wires the same in-memory collaborators and projection singleton-sharing as
/// RngHelpdesk.Api/Program.cs's Development composition, so dispatching a handler's
/// events updates every read store exactly as it would in production.
/// </summary>
internal sealed class OperationsTestFixture
{
    public InMemUserRepository UserRepository { get; } = new();
    public InMemoryEventStore EventStore { get; } = new();
    public InMemoryCredentialStore CredentialStore { get; } = new();
    public RankResolver RankResolver { get; } = new(new InMemoryRankThresholdProvider());

    public UserSummaryProjection UserSummaryProjection { get; }
    public PointHistoryProjection PointHistoryProjection { get; }
    public UserLifecycleHistoryProjection UserLifecycleHistoryProjection { get; } = new();
    public RunescapeAccountHistoryProjection RunescapeAccountHistoryProjection { get; } = new();

    public IEventDispatcher EventDispatcher { get; }
    public IUserRoleService UserRoleService { get; }

    public OperationsTestFixture()
    {
        UserSummaryProjection = new UserSummaryProjection(RankResolver);
        PointHistoryProjection = new PointHistoryProjection(RankResolver);

        EventDispatcher = new InMemEventDispatcher(new object[]
        {
            PointHistoryProjection,
            UserSummaryProjection,
            UserLifecycleHistoryProjection,
            RunescapeAccountHistoryProjection
        });

        UserRoleService = new UserRoleService(EventStore);
    }

    /// <summary>
    /// Creates a User via the real aggregate factory, saves it to the repository, and dispatches
    /// the resulting events through every projection, mirroring how Program.cs seeds users.
    /// </summary>
    public User CreateAndDispatchUser(
        ulong actingUserId,
        DiscordAccount discordAccount,
        IEnumerable<RunescapeAccount>? runescapeAccounts = null)
    {
        var user = User.Create(actingUserId, discordAccount, runescapeAccounts ?? []);
        var events = UserRepository.Save(user);
        EventDispatcher.Dispatch(events);
        return user;
    }
}

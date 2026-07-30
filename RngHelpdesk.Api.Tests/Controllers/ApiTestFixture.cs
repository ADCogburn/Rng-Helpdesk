using RngHelpdesk.Api.Validators.Users;
using RngHelpdesk.Contracts.Common.Ranks;
using RngHelpdesk.Domain.Users;
using RngHelpdesk.Infrastructure.Common;
using RngHelpdesk.Infrastructure.Persistence.EventStore;
using RngHelpdesk.Infrastructure.Persistence.Points;
using RngHelpdesk.Infrastructure.Points;
using RngHelpdesk.Infrastructure.Security;
using RngHelpdesk.Infrastructure.Users;
using RngHelpdesk.Infrastructure.Users.RunescapeAccount;
using RngHelpdesk.Operations.Admin;
using RngHelpdesk.Operations.Points;
using RngHelpdesk.Operations.Services;
using RngHelpdesk.Operations.Users;
using RngHelpdesk.Operations.Users.RunescapeAccounts;

namespace RngHelpdesk.Api.Tests.Controllers;

/// <summary>
/// Wires the same in-memory collaborators and projection singleton-sharing as
/// RngHelpdesk.Api/Program.cs's Development composition (mirrors
/// RngHelpdesk.Operations.Tests/OperationsTestFixture.cs), plus factory methods for the real
/// handler instances each controller depends on.
/// </summary>
internal sealed class ApiTestFixture
{
    public InMemUserRepository UserRepository { get; } = new();
    public InMemoryEventStore EventStore { get; } = new();
    public InMemoryCredentialStore CredentialStore { get; } = new();
    public RankResolver RankResolver { get; } = new(new InMemoryRankThresholdProvider().GetThresholdsAsync().GetAwaiter().GetResult());

    /// <summary>
    /// Separate instance from the one used to build <see cref="RankResolver"/> above, mirroring
    /// Program.cs's split between the frozen startup snapshot and the scoped provider used per
    /// request -- edits made through this instance don't retroactively affect RankResolver.
    /// </summary>
    public InMemoryRankThresholdProvider RankThresholdProvider { get; } = new();

    public UserSummaryProjection UserSummaryProjection { get; }
    public PointHistoryProjection PointHistoryProjection { get; }
    public UserLifecycleHistoryProjection UserLifecycleHistoryProjection { get; } = new();
    public RunescapeAccountHistoryProjection RunescapeAccountHistoryProjection { get; } = new();

    public IEventDispatcher EventDispatcher { get; }
    public IUserRoleService UserRoleService { get; }

    public ApiTestFixture()
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
    public async Task<User> CreateAndDispatchUserAsync(
        ulong actingUserId,
        DiscordAccount discordAccount,
        IEnumerable<RunescapeAccount>? runescapeAccounts = null)
    {
        var user = User.Create(actingUserId, discordAccount, runescapeAccounts ?? []);
        var events = await UserRepository.SaveAsync(user);
        EventDispatcher.Dispatch(events);
        return user;
    }

    // -- Admin --

    public CreateUserHandler CreateCreateUserHandler()
        => new(UserSummaryProjection, UserRepository, EventDispatcher, CredentialStore);

    public ChangeUserRoleHandler CreateChangeUserRoleHandler()
        => new(UserRoleService, UserSummaryProjection, EventDispatcher);

    // -- Ranks --

    public GetRankThresholdsHandler CreateGetRankThresholdsHandler()
        => new(RankThresholdProvider);

    public UpdateRankThresholdHandler CreateUpdateRankThresholdHandler()
        => new(RankThresholdProvider, RankThresholdProvider);

    // -- Users --

    public GetAllUsersHandler CreateGetAllUsersHandler()
        => new(UserSummaryProjection);

    public GetUserByIdHandler CreateGetUserByIdHandler()
        => new(UserSummaryProjection);

    public GetUserByRunescapeUsernameHandler CreateGetUserByRunescapeUsernameHandler()
        => new(UserSummaryProjection);

    public GetUsersHandler CreateGetUsersHandler()
        => new(RunescapeAccountHistoryProjection, UserSummaryProjection);

    public GetUserLifecycleHistoryHandler CreateGetUserLifecycleHistoryHandler()
        => new(UserLifecycleHistoryProjection, UserSummaryProjection);

    // -- Points --

    public AddPointsToUserHandler CreateAddPointsToUserHandler()
        => new(UserRepository, EventDispatcher);

    public RemovePointsFromUserHandler CreateRemovePointsFromUserHandler()
        => new(UserRepository, EventDispatcher);

    public GetPointHistoryForUserHandler CreateGetPointHistoryForUserHandler()
        => new(PointHistoryProjection, UserSummaryProjection);

    // -- Runescape accounts --

    public GetRunescapeAccountHandler CreateGetRunescapeAccountHandler()
        => new(UserSummaryProjection);

    public GetPreviousRunescapeAccountsHandler CreateGetPreviousRunescapeAccountsHandler()
        => new(RunescapeAccountHistoryProjection);

    public GetRunescapeAccountHistoryHandler CreateGetRunescapeAccountHistoryHandler()
        => new(RunescapeAccountHistoryProjection);

    public LinkRunescapeAccountHandler CreateLinkRunescapeAccountHandler()
        => new(UserRepository, EventDispatcher, new LinkRunescapeAccountRequestValidator());

    public DelinkRunescapeAccountHandler CreateDelinkRunescapeAccountHandler()
        => new(UserRepository, EventDispatcher);

    public RenameRunescapeAccountHandler CreateRenameRunescapeAccountHandler()
        => new(UserRepository, EventDispatcher);
}

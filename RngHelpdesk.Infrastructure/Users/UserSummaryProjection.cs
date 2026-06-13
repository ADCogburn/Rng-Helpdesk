using RngHelpdesk.Contracts.Common.Ranks;
using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Contracts.Users.Views;
using RngHelpdesk.Domain.Users.Events;
using RngHelpdesk.Infrastructure.Common;
using RngHelpdesk.Infrastructure.Security;

namespace RngHelpdesk.Infrastructure.Users;

public sealed class UserSummaryProjection(RankResolver rankResolver) :
    IProjectionState,
    IProjectionHandler<UserCreatedEvent>,
    IProjectionHandler<UserDeactivatedEvent>,
    IProjectionHandler<UserReactivatedEvent>,
    IProjectionHandler<ClanPointsChangedEvent>,
    IProjectionHandler<UserAppRoleChangedEvent>,
    IProjectionHandler<RunescapeAccountLinkedEvent>,
    IProjectionHandler<RunescapeAccountDelinkedEvent>,
    IProjectionHandler<RunescapeAccountRenamedEvent>,
    IUserSummaryReadStore,
    IUserLookupReadStore
{
    private readonly Dictionary<ulong, UserSummaryReadModel> _users = new();
    // Instead of scanning through every RSN of every account in the projection, simply keep the data in-memory for a quick search.
    private readonly Dictionary<string, ulong> _runescapeUsernameIndex = new(StringComparer.OrdinalIgnoreCase);
    private readonly RankResolver _rankResolver = rankResolver;

    public bool IsEmpty => _users.Count == 0;

    public IReadOnlyCollection<UserSummaryReadModel> GetAll() => _users.Values;

    public bool TryGetById(ulong userId, out UserSummaryReadModel? user)
    {
        return _users.TryGetValue(userId, out user);
    }

    public bool TryGetByRunescapeUsername(string username, out UserSummaryReadModel? user)
    {
        user = null;

        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username must be provided.", nameof(username));

        if (!_runescapeUsernameIndex.TryGetValue(username, out var userId))
            return false;

        return _users.TryGetValue(userId, out user);
    }

    public bool ExistsWithDiscordId(ulong discordId)
    {
        return _users.ContainsKey(discordId);
    }

    public bool ExistsWithDiscordUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username must be provided.", nameof(username));

        var user = _users.Values.FirstOrDefault(u => u.DiscordAccount?.Username.Equals(username, StringComparison.OrdinalIgnoreCase) == true);

        return user != null;
    }

    public bool ExistsWithRunescapeUsername(string rsn)
    {
        if (string.IsNullOrWhiteSpace(rsn))
            throw new ArgumentException("Username must be provided.", nameof(rsn));

        return _runescapeUsernameIndex.ContainsKey(rsn);
    }

    #region Projections

    public void Project(UserCreatedEvent e)
    {
        // Defaults for new users - they can be changed by later events in the same stream or in future events.
        AppRole appRole = AppRole.Member;
        int clanPoints = 0;

        _users[e.UserId] = new UserSummaryReadModel
        (
            UserId: e.UserId,
            IsActive: true,
            DateCreated: e.OccurredAt,
            ClanPoints: clanPoints,
            AppRole: appRole,
            Rank: _rankResolver.Resolve(appRole, clanPoints),
            RunescapeAccounts: e.RunescapeAccounts
                .Select(a => new RunescapeAccountView
                (
                    Username: a.Username
                ))
                .ToList(),
            DiscordAccount: new DiscordAccountView
            (
                DiscordId: e.DiscordAccount.DiscordId,
                Username: e.DiscordAccount.Username
            )
        );

        foreach (var account in e.RunescapeAccounts)
        {
            _runescapeUsernameIndex[account.Username] = e.UserId;
        }
    }

    public void Project(UserDeactivatedEvent e)
    {
        if (!_users.TryGetValue(e.UserId, out var existing))
            return;

        _users[e.UserId] = existing with
        {
            IsActive = false
        };
    }

    public void Project(UserReactivatedEvent e)
    {
        if (!_users.TryGetValue(e.UserId, out var existing))
            return;

        _users[e.UserId] = existing with
        {
            IsActive = true
        };
    }

    public void Project(ClanPointsChangedEvent e)
    {
        if (!_users.TryGetValue(e.UserId, out var existing))
            return;

        var newPoints = existing.ClanPoints + e.Delta;

        _users[e.UserId] = existing with
        {
            ClanPoints = newPoints,
            Rank = _rankResolver.Resolve(existing.AppRole, newPoints)
        };
    }

    public void Project(UserAppRoleChangedEvent e)
    {
        if (!_users.TryGetValue(e.UserId, out var existing))
            return;

        _users[e.UserId] = existing with
        {
            AppRole = e.NewRole,
            Rank = _rankResolver.Resolve(e.NewRole, existing.ClanPoints)
        };
    }

    public void Project(RunescapeAccountLinkedEvent e)
    {
        if (!_users.TryGetValue(e.UserId, out var existing))
        {
            return;
        }

        if (existing.RunescapeAccounts.Any(a =>
            a.Username.Equals(e.Username, StringComparison.OrdinalIgnoreCase)))
            return;

        var updatedAccounts = existing.RunescapeAccounts
            .Append(new RunescapeAccountView(e.Username))
            .ToList();

        _users[e.UserId] = existing with
        {
            RunescapeAccounts = updatedAccounts
        };

        _runescapeUsernameIndex[e.Username] = e.UserId;
    }

    public void Project(RunescapeAccountDelinkedEvent e)
    {
        if (!_users.TryGetValue(e.UserId, out var existing))
            return;

        var updatedAccounts = existing.RunescapeAccounts
            .Where(a => !a.Username.Equals(e.Username, StringComparison.OrdinalIgnoreCase))
            .ToList();

        _users[e.UserId] = existing with
        {
            RunescapeAccounts = updatedAccounts
        };

        _runescapeUsernameIndex.Remove(e.Username);
    }

    public void Project(RunescapeAccountRenamedEvent e)
    {
        if (!_users.TryGetValue(e.UserId, out var existing))
            return;

        var updatedAccounts = existing.RunescapeAccounts
            .Where(a => !a.Username.Equals(e.OldUsername, StringComparison.OrdinalIgnoreCase))
            .Append(new RunescapeAccountView(e.NewUsername))
            .ToList();

        _users[e.UserId] = existing with
        {
            RunescapeAccounts = updatedAccounts
        };

        _runescapeUsernameIndex.Remove(e.OldUsername);
        _runescapeUsernameIndex[e.NewUsername] = e.UserId;
    }

    #endregion
}
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
    IProjectionHandler<DiscordAccountLinkedEvent>,
    IProjectionHandler<DiscordAccountDelinkedEvent>,
    IProjectionHandler<RunescapeAccountLinkedEvent>,
    IProjectionHandler<RunescapeAccountDelinkedEvent>,
    IProjectionHandler<RunescapeAccountRenamedEvent>
{
    private readonly Dictionary<ulong, UserSummaryReadModel> _users = new();
    private readonly RankResolver _rankResolver = rankResolver;

    public bool IsEmpty => _users.Count == 0;

    public IReadOnlyCollection<UserSummaryReadModel> GetAll() => _users.Values;

    public UserSummaryReadModel GetSingleById(ulong userId)
    {
        if (!_users.TryGetValue(userId, out var user))
            throw new InvalidOperationException($"User {userId} not found.");

        return user;
    }

    public UserSummaryReadModel GetByDiscordId(ulong discordId)
    {
        return GetSingleById(discordId);
    }

    public UserSummaryReadModel GetByRunescapeUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username must be provided.", nameof(username));

        var user = _users.Values.FirstOrDefault(u =>
            u.RunescapeAccounts.Any(r =>
                r.Username.Equals(username, StringComparison.OrdinalIgnoreCase)));

        if (user is null)
            throw new InvalidOperationException(
                $"No user linked to Runescape account '{username}'");

        return user;
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

    public void Project(DiscordAccountLinkedEvent e)
    {
        if (!_users.TryGetValue(e.UserId, out var existing))
        {
            existing = new UserSummaryReadModel
            {
                UserId = e.UserId,
                AuthorityRole = AuthorityRole.Member,
                IsActive = true,
                DateCreated = e.OccurredAt,
                RunescapeAccounts = [],
                DiscordAccount = []
            };
        }

        var updatedAccounts = existing.DiscordAccount
            .Append(new DiscordAccountView
            {
                DiscordId = e.DiscordId,
                Username = e.Username,
                IsActive = true
            })
            .ToList();

        _users[e.UserId] = existing with
        {
            DiscordAccount = updatedAccounts
        };
    }

    public void Project(DiscordAccountDelinkedEvent e)
    {
        if (!_users.TryGetValue(e.UserId, out var existing))
            return;

        var updatedAccounts = existing.DiscordAccount
            .Where(a => a.DiscordId != e.DiscordId)
            .ToList();

        _users[e.UserId] = existing with
        {
            DiscordAccount = updatedAccounts
        };
    }

    public void Project(RunescapeAccountLinkedEvent e)
    {
        if (!_users.TryGetValue(e.UserId, out var existing))
        {
            // Replay from checkpoint can deliver later events before UserCreatedEvent
            // when projection state was lost (e.g. app restart). Create a minimal stub.
            existing = new UserSummaryReadModel
            {
                UserId = e.UserId,
                AuthorityRole = AuthorityRole.Member,
                IsActive = true,
                DateCreated = e.OccurredAt,
                RunescapeAccounts = [],
                DiscordAccount = []
            };
        }

        var updatedAccounts = existing.RunescapeAccounts
            .Append(new RunescapeAccountView
            {
                Username = e.Username
            })
            .ToList();

        _users[e.UserId] = existing with
        {
            RunescapeAccounts = updatedAccounts
        };
    }

    public void Project(RunescapeAccountDelinkedEvent e)
    {
        if (!_users.TryGetValue(e.UserId, out var existing))
            return;

        var updatedAccounts = existing.RunescapeAccounts
            .Where(a =>
                !a.Username.Equals(e.Username, StringComparison.OrdinalIgnoreCase))
            .ToList();

        _users[e.UserId] = existing with
        {
            RunescapeAccounts = updatedAccounts
        };
    }

    public void Project(RunescapeAccountRenamedEvent e)
    {
        if (!_users.TryGetValue(e.UserId, out var existing))
            return;

        var updatedAccounts = existing.RunescapeAccounts
            .Where(a => !a.Username.Equals(e.OldUsername, StringComparison.OrdinalIgnoreCase))
            .Append(new RunescapeAccountView
            {
                Username = e.NewUsername
            })
            .ToList();

        _users[e.UserId] = existing with
        {
            RunescapeAccounts = updatedAccounts
        };
    }

    #endregion
}
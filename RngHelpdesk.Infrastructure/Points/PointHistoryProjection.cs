using RngHelpdesk.Domain.Users;
using RngHelpdesk.Domain.Users.Events;
using RngHelpdesk.Infrastructure.Common;
using RngHelpdesk.Contracts.Points.Views;
using RngHelpdesk.Contracts.Common.Ranks;

namespace RngHelpdesk.Infrastructure.Points;

public sealed class PointHistoryProjection :
    IProjectionHandler<UserCreatedEvent>,
    IProjectionHandler<AuthorityRoleChangedEvent>,
    IProjectionHandler<ClanPointsChangedEvent>
{
    private sealed class UserPointState
    {
        public int TotalPoints;
        public AuthorityRole AuthorityRole;
        public List<PointHistoryItem> History = new();
    }

    private readonly Dictionary<int, UserPointState> _store = new();
    private readonly RankResolver _rankResolver;

    public PointHistoryProjection(RankResolver rankResolver)
    {
        _rankResolver = rankResolver;
    }

    public void Project(UserCreatedEvent e)
    {
        if (_store.ContainsKey(e.UserId))
            return;

        Rank? initialRank = e.AuthorityRole == AuthorityRole.Member
            ? _rankResolver.Resolve(AuthorityRole.Member, 0)
            : null;

        _store[e.UserId] = new UserPointState
        {
            TotalPoints = 0,
            AuthorityRole = e.AuthorityRole,
            History =
            {
                new PointHistoryItem
                {
                    Delta = 0,
                    Reason = "Account created",
                    OccurredAt = e.OccurredAt,
                    RankBefore = null,
                    RankAfter = initialRank
                }
            }
        };
    }

    public void Project(AuthorityRoleChangedEvent e)
    {
        if (!_store.TryGetValue(e.UserId, out var state))
            return;

        var rankBefore = GetDisplayedRank(state);

        state.AuthorityRole = e.NewRole;

        var rankAfter = GetDisplayedRank(state);

        state.History.Add(new PointHistoryItem
        {
            Delta = 0,
            Reason = $"Authority role changed: {e.OldRole} → {e.NewRole}",
            OccurredAt = e.OccurredAt,
            RankBefore = rankBefore,
            RankAfter = rankAfter
        });
    }

    public void Project(ClanPointsChangedEvent e)
    {
        if (!_store.TryGetValue(e.UserId, out var state))
        {
            throw new InvalidOperationException(
                $"PointHistoryProjection received ClanPointsChangedEvent for user {e.UserId} before UserCreatedEvent.");
        }

        var rankBefore = GetDisplayedRank(state);

        state.TotalPoints += e.Delta;   // ✅ ADD ONCE

        var rankAfter = GetDisplayedRank(state);

        state.History.Add(new PointHistoryItem
        {
            Delta = e.Delta,
            Reason = e.Reason,
            OccurredAt = e.OccurredAt,
            RankBefore = rankBefore,
            RankAfter = rankAfter
        });
    }

    public IReadOnlyList<PointHistoryItem> GetForUser(int userId)
        => _store.TryGetValue(userId, out var state)
            ? state.History
            : Array.Empty<PointHistoryItem>();

    public int GetCountForUser(int userId)
        => _store.TryGetValue(userId, out var state)
            ? state.History.Count
            : 0;

    private Rank? GetDisplayedRank(UserPointState state)
    {
        return state.AuthorityRole == AuthorityRole.Member
            ? _rankResolver.Resolve(AuthorityRole.Member, state.TotalPoints)
            : RankHelper.FromAuthority(state.AuthorityRole);
    }
}
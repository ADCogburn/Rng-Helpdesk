using RngHelpdesk.Contracts.Common.Ranks;
using RngHelpdesk.Contracts.Points.Views;
using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Domain.Users.Events;
using RngHelpdesk.Infrastructure.Common;
using RngHelpdesk.Infrastructure.Security;

namespace RngHelpdesk.Infrastructure.Points;

public sealed class PointHistoryProjection :
    IProjectionState,
    IProjectionHandler<UserCreatedEvent>,
    IProjectionHandler<ClanPointsChangedEvent>,
    IProjectionHandler<UserAppRoleChangedEvent>,
    IPointHistoryReadStore
{
    private sealed class UserPointState
    {
        public int TotalPoints;
        public AppRole AppRole = AppRole.Member;
        public List<PointHistoryItem> History = new();
    }

    private readonly Dictionary<ulong, UserPointState> _store = new();
    private readonly RankResolver _rankResolver;
    private readonly object _lock = new();

    public bool IsEmpty
    {
        get { lock (_lock) { return _store.Count == 0; } }
    }

    public PointHistoryProjection(RankResolver rankResolver)
    {
        _rankResolver = rankResolver;
    }

    public IReadOnlyList<PointHistoryItem> GetPointHistoryForUser(ulong userId)
    {
        lock (_lock)
        {
            return _store.TryGetValue(userId, out var state)
                ? state.History.ToList()
                : Array.Empty<PointHistoryItem>();
        }
    }

    public int GetCountForUser(ulong userId)
    {
        lock (_lock)
        {
            return _store.TryGetValue(userId, out var state)
                ? state.History.Count
                : 0;
        }
    }

    private Rank? GetDisplayedRank(UserPointState state)
    {
        return _rankResolver.Resolve(state.AppRole, state.TotalPoints);
    }

    #region Projections

    public void Project(UserCreatedEvent e)
    {
        lock (_lock)
        {
            if (_store.ContainsKey(e.UserId))
                return;

            var initialRole = AppRole.Member;
            var initialRank = _rankResolver.Resolve(initialRole, 0);

            _store[e.UserId] = new UserPointState
            {
                TotalPoints = 0,
                AppRole = initialRole,
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
    }

    public void Project(UserAppRoleChangedEvent e)
    {
        lock (_lock)
        {
            if (!_store.TryGetValue(e.UserId, out var state))
                return;

            var rankBefore = GetDisplayedRank(state);

            state.AppRole = e.NewRole;

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
    }

    public void Project(ClanPointsChangedEvent e)
    {
        lock (_lock)
        {
            if (!_store.TryGetValue(e.UserId, out var state))
                return;

            var rankBefore = GetDisplayedRank(state);

            state.TotalPoints += e.Delta;

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
    }

    #endregion
}
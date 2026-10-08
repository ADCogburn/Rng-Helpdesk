namespace RngHelpdesk.DiscordBot.Api.Models;

// DTOs mirroring the RngHelpdesk.Api contract (docs/ui/PLAN.md). The bot is standalone and does not
// reference RngHelpdesk.Contracts. Ids are ulong here and strings on the wire (see ApiJson).

public enum AppRole { Member, Administrator, SuperAdministrator, Owner }

public enum Rank
{
    Bronze, Iron, Steel, Mithril, Adamant, Rune, Dragon, Sapphire, Emerald, Ruby, Diamond, Dragonstone, Onyx, Zenyte,
    Administrator, DeputyOwner, Owner,
}

// --- auth ---
public sealed record BotTokenRequest(string ApiKey);
public sealed record BotTokenResponse(string Token, DateTimeOffset ExpiresAt);
public sealed record DiscordExchangeRequest(ulong DiscordId);
public sealed record DiscordExchangeResponse(string Token, DateTimeOffset ExpiresAt, AppRole AppRole);

// --- users ---
public sealed record DiscordAccountDto(ulong DiscordId, string Username);
public sealed record RunescapeAccountDto(string Username);

public sealed record UserResponse(
    ulong Id,
    AppRole AppRole,
    long ClanPoints,
    Rank Rank,
    bool IsActive,
    DateTimeOffset DateCreated,
    DiscordAccountDto DiscordAccount,
    IReadOnlyList<RunescapeAccountDto> RunescapeAccounts);

public sealed record UsersByHistoricalRsnResponse(IReadOnlyList<UserResponse> Users);

public sealed record LifecycleHistoryItem(string Action, DateTimeOffset OccurredAt);
public sealed record UserLifecycleResponse(ulong UserId, IReadOnlyList<LifecycleHistoryItem> History);

public sealed record AdjustPointsRequest(int Points, string Reason);
public sealed record PointHistoryEvent(long Delta, string Reason, DateTimeOffset OccurredAt, Rank? RankBefore, Rank? RankAfter);
public sealed record PointHistoryResponse(ulong UserId, int TotalEventCount, IReadOnlyList<PointHistoryEvent> Events);

// --- runescape accounts ---
public sealed record RunescapeAccountsResponse(IReadOnlyList<RunescapeAccountDto> Accounts);
public sealed record RunescapeAccountHistoryItem(string ChangeType, string? Username, string? OldUsername, string? NewUsername, DateTimeOffset OccurredAt);
public sealed record RunescapeAccountHistoryResponse(IReadOnlyList<RunescapeAccountHistoryItem> History);
public sealed record RunescapeUsernameRequest(string Username);
public sealed record RenameRunescapeAccountRequest(string OldUsername, string NewUsername);

// --- admin ---
public sealed record CreateUserRequest(DiscordAccountDto DiscordAccount, IReadOnlyList<RunescapeAccountDto> RunescapeAccounts);
public sealed record CreateUserResponse(ulong UserId, string Username, string TemporaryPassword);

// --- rank thresholds ---
public sealed record RankThresholdDto(Rank Rank, int PointsRequired);
public sealed record RankThresholdsResponse(IReadOnlyList<RankThresholdDto> Thresholds);
public sealed record UpdateRankThresholdRequest(int PointsRequired);

// --- public ---
public sealed record RankDistributionItem(Rank Rank, int Count);
public sealed record PublicOverviewResponse(int ActiveMemberCount, long TotalClanPoints, IReadOnlyList<RankDistributionItem> RankDistribution);
public sealed record PublicRanksResponse(IReadOnlyList<RankThresholdDto> Ranks);
public sealed record LeaderboardEntry(int Position, string RunescapeUsername, Rank Rank, long ClanPoints);
public sealed record PublicLeaderboardResponse(IReadOnlyList<LeaderboardEntry> Entries);

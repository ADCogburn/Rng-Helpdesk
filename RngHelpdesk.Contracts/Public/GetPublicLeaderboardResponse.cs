using RngHelpdesk.Contracts.Common.Ranks;

namespace RngHelpdesk.Contracts.Public;

public sealed class GetPublicLeaderboardResponse
{
    public IReadOnlyList<PublicLeaderboardItem> Entries { get; init; } = [];
}

public sealed record PublicLeaderboardItem(int Position, string RunescapeUsername, Rank Rank, int ClanPoints);

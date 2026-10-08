namespace RngHelpdesk.Contracts.Public;

public sealed class GetPublicLeaderboardQuery
{
    public const int DefaultTop = 25;
    public const int MaxTop = 100;

    public int Top { get; init; } = DefaultTop;
}

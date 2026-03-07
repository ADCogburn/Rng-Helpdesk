namespace RngHelpdesk.Contracts.Common.Ranks;

public sealed class RankThreshold
{
    public Rank Rank { get; }
    public int PointsRequired { get; }

    public RankThreshold(Rank rank, int pointsRequired)
    {
        Rank = rank;
        PointsRequired = Math.Max(0, pointsRequired);
    }
}
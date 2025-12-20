namespace RngHelpdesk.Domain.Ranks;

public sealed class RankThreshold
{
    public Rank Rank { get; }
    public int PointsRequired { get; }

    public RankThreshold(Rank rank, int pointsRequired)
    {
        Rank = rank;
        PointsRequired = pointsRequired;
        PointsRequired = Math.Max(0, pointsRequired);
    }
}
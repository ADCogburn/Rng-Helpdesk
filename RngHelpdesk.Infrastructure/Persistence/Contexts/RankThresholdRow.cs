namespace RngHelpdesk.Infrastructure.Persistence.Contexts;

public sealed class RankThresholdRow
{
    public string Rank { get; set; } = default!;
    public int PointsRequired { get; set; }
    public int SortOrder { get; set; }
}

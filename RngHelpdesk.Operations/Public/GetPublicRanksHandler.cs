using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Common.Ranks;
using RngHelpdesk.Contracts.Common.Ranks.Views;
using RngHelpdesk.Contracts.Public;
using RngHelpdesk.Operations.Common;

namespace RngHelpdesk.Operations.Public;

public sealed class GetPublicRanksHandler(
    IRankThresholdProvider rankThresholdProvider) : IQueryHandler<GetPublicRanksQuery, GetPublicRanksResponse>
{
    public async Task<QueryResult<GetPublicRanksResponse>> Handle(GetPublicRanksQuery query, CancellationToken cancellationToken = default)
    {
        var thresholds = await rankThresholdProvider.GetThresholdsAsync(cancellationToken);

        return QueryResult<GetPublicRanksResponse>.Ok(new GetPublicRanksResponse
        {
            Ranks = thresholds
                .OrderBy(t => t.PointsRequired)
                .Select(t => new RankThresholdView(t.Rank, t.PointsRequired))
                .ToList()
        });
    }
}

using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Common.Ranks;
using RngHelpdesk.Contracts.Common.Ranks.Queries;
using RngHelpdesk.Contracts.Common.Ranks.Views;
using RngHelpdesk.Operations.Common;

namespace RngHelpdesk.Operations.Admin;

public sealed class GetRankThresholdsHandler(
    IRankThresholdProvider rankThresholdProvider) : IQueryHandler<GetRankThresholdsQuery, GetRankThresholdsResponse>
{
    public async Task<QueryResult<GetRankThresholdsResponse>> Handle(GetRankThresholdsQuery query, CancellationToken cancellationToken = default)
    {
        var thresholds = await rankThresholdProvider.GetThresholdsAsync(cancellationToken);

        return QueryResult<GetRankThresholdsResponse>.Ok(new GetRankThresholdsResponse
        {
            Thresholds = thresholds
                .Select(t => new RankThresholdView(t.Rank, t.PointsRequired))
                .ToList()
        });
    }
}

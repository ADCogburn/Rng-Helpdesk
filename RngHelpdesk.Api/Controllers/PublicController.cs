using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RngHelpdesk.Contracts.Public;
using RngHelpdesk.Operations.Common;

namespace RngHelpdesk.Api.Controllers;

/// <summary>
/// Anonymous, read-only endpoints for the public landing page. Responses never include user
/// ids or Discord data.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("public")]
public sealed class PublicController(
    IQueryHandler<GetPublicOverviewQuery, GetPublicOverviewResponse> getOverviewHandler,
    IQueryHandler<GetPublicRanksQuery, GetPublicRanksResponse> getRanksHandler,
    IQueryHandler<GetPublicLeaderboardQuery, GetPublicLeaderboardResponse> getLeaderboardHandler) : ControllerBase
{
    /// <summary>
    /// Gets clan-wide headline stats: active member count, total clan points and members per rank.
    /// </summary>
    [HttpGet("overview")]
    public async Task<ActionResult<GetPublicOverviewResponse>> GetOverview(CancellationToken cancellationToken)
    {
        var result = await getOverviewHandler.Handle(new GetPublicOverviewQuery(), cancellationToken);

        if (!result.Success)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets the point-based rank ladder, ascending by points required.
    /// </summary>
    [HttpGet("ranks")]
    public async Task<ActionResult<GetPublicRanksResponse>> GetRanks(CancellationToken cancellationToken)
    {
        var result = await getRanksHandler.Handle(new GetPublicRanksQuery(), cancellationToken);

        if (!result.Success)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets the top active members by clan points (<paramref name="top"/> is clamped to 1..100, default 25).
    /// </summary>
    [HttpGet("leaderboard")]
    public async Task<ActionResult<GetPublicLeaderboardResponse>> GetLeaderboard(
        [FromQuery] int top = GetPublicLeaderboardQuery.DefaultTop,
        CancellationToken cancellationToken = default)
    {
        var result = await getLeaderboardHandler.Handle(new GetPublicLeaderboardQuery { Top = top }, cancellationToken);

        if (!result.Success)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }
}

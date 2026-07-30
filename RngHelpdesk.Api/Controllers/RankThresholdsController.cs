using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RngHelpdesk.Api.DTOs;
using RngHelpdesk.Api.Helpers;
using RngHelpdesk.Api.Security;
using RngHelpdesk.Contracts.Common.Ranks;
using RngHelpdesk.Contracts.Common.Ranks.Commands;
using RngHelpdesk.Contracts.Common.Ranks.Queries;
using RngHelpdesk.Operations.Common;

namespace RngHelpdesk.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthPolicies.AdminPlus)]
[Route("[controller]")]
public sealed class RankThresholdsController(
    IQueryHandler<GetRankThresholdsQuery, GetRankThresholdsResponse> getRankThresholdsHandler,
    ICommandHandler<UpdateRankThresholdCommand> updateRankThresholdHandler) : ControllerBase
{
    /// <summary>
    /// Gets every rank's configured clan-point threshold.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<GetRankThresholdsResponse>> GetRankThresholds(CancellationToken cancellationToken)
    {
        var result = await getRankThresholdsHandler.Handle(new GetRankThresholdsQuery(), cancellationToken);

        if (!result.Success)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    /// <summary>
    /// Updates the clan-point threshold required for a rank.
    /// </summary>
    [HttpPut("{rank}")]
    public async Task<IActionResult> UpdateRankThreshold(Rank rank, [FromBody] UpdateRankThresholdDto request, CancellationToken cancellationToken)
    {
        var command = new UpdateRankThresholdCommand
        (
            ActingUserId: User.GetUserId(),
            Rank: rank,
            PointsRequired: request.PointsRequired
        );

        var result = await updateRankThresholdHandler.Handle(command, cancellationToken);

        if (!result.Success)
            return BadRequest(result.Error);

        return NoContent();
    }
}

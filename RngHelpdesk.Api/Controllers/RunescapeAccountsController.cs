using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RngHelpdesk.Api.DTOs;
using RngHelpdesk.Api.Helpers;
using RngHelpdesk.Api.Security;
using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Models.Users.Dtos;
using RngHelpdesk.Contracts.Users.Commands;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Operations.Common;

namespace RngHelpdesk.Api.Controllers;

[Authorize(Policy = AuthPolicies.AdminPlus)]
[ApiController]
[Route("users/{userId:long}/runescape-accounts")]
public class RunescapeAccountsController(
    IQueryHandler<GetRunescapeAccountsQuery, GetRunescapeAccountsResponse> getRunescapeHandler,
    IQueryHandler<GetPreviousRunescapeAccountsQuery, GetRunescapeAccountsResponse> getPreviousRunescapeAccountsHandler,
    IQueryHandler<GetRunescapeAccountHistoryQuery, GetRunescapeAccountHistoryResponse> getRunescapeAccountHistoryHandler,
    ICommandHandler<LinkRunescapeAccountRequest> linkRunescapeAccountHandler,
    ICommandHandler<DelinkRunescapeAccountRequest> delinkRunescapeHandler,
    ICommandHandler<RenameRunescapeAccountRequest> renameRunescapeHandler) : ControllerBase
{
    /// <summary>
    /// Returns only the Runescape accounts for a certain user.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> GetRunescapeAccounts(ulong userId, CancellationToken cancellationToken)
    {
        var query = new GetRunescapeAccountsQuery()
        {
            UserId = userId
        };

        var result = await getRunescapeHandler.Handle(query, cancellationToken);

        return result.Status switch
        {
            ResultStatus.NotFound => NotFound(result.Error),
            ResultStatus.Success => Ok(result.Value),
            _ => BadRequest(result.Error)
        };
    }

    /// <summary>
    /// Gets the full list of previously user RSNs by a user.
    /// </summary>
    [HttpGet("previous")]
    public async Task<ActionResult<GetRunescapeAccountsResponse>> GetPreviousRunescapeAccountUsernames(ulong userId, CancellationToken cancellationToken)
    {
        var query = new GetPreviousRunescapeAccountsQuery(userId);

        var result = await getPreviousRunescapeAccountsHandler.Handle(query, cancellationToken);

        return result.Status switch
        {
            ResultStatus.NotFound => NotFound(result.Error),
            ResultStatus.Success => Ok(result.Value),
            _ => BadRequest(result.Error)
        };
    }

    /// <summary>
    /// Adds a Runescape account username to a users list of active accounts.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> LinkRunescapeAccount(ulong userId, [FromBody] RunescapeAccountDto username, CancellationToken cancellationToken)
    {
        var request = new LinkRunescapeAccountRequest(
            ActingUserId: User.GetUserId(),
            UserId: userId,
            Username: username.Username);

        var result = await linkRunescapeAccountHandler.Handle(request, cancellationToken);

        return result.Status switch
        {
            ResultStatus.NotFound => NotFound(result.Error),
            ResultStatus.Success => Ok(),
            _ => BadRequest(result.Error)
        };
    }

    /// <summary>
    /// Removes a Runescape account username from a users active accounts.
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> DelinkRunescapeAccount(ulong userId, [FromBody] RunescapeAccountDto username, CancellationToken cancellationToken)
    {
        var request = new DelinkRunescapeAccountRequest(
            ActingUserId: User.GetUserId(),
            UserId: userId,
            Username: username.Username);

        var result = await delinkRunescapeHandler.Handle(request, cancellationToken);

        return result.Status switch
        {
            ResultStatus.NotFound => NotFound(result.Error),
            ResultStatus.Success => Ok(),
            _ => BadRequest(result.Error)
        };
    }

    /// <summary>
    /// Renames a Runescape account username for a user.
    /// </summary>
    [HttpPut("rename")]
    public async Task<IActionResult> RenameRunescapeAccount(ulong userId, [FromBody] RenameRunescapeAccountDto body, CancellationToken cancellationToken)
    {
        var request = new RenameRunescapeAccountRequest(
            ActingUserId: User.GetUserId(),
            UserId: userId,
            OldUsername: body.OldUsername,
            NewUsername: body.NewUsername);

        var result = await renameRunescapeHandler.Handle(request, cancellationToken);

        return result.Status switch
        {
            ResultStatus.NotFound => NotFound(result.Error),
            ResultStatus.Success => NoContent(),
            _ => BadRequest(result.Error)
        };
    }

    /// <summary>
    /// Gets the full history of Runescape accounts for a user, including linked, delinked, and renamed accounts.
    /// </summary>
    [HttpGet("history")]
    public async Task<ActionResult<GetRunescapeAccountHistoryResponse>> GetRunescapeAccountHistory(ulong userId, CancellationToken cancellationToken)
    {
        var query = new GetRunescapeAccountHistoryQuery(userId);

        var result = await getRunescapeAccountHistoryHandler.Handle(query, cancellationToken);

        return result.Status switch
        {
            ResultStatus.NotFound => NotFound(result.Error),
            ResultStatus.Success => Ok(result.Value),
            _ => BadRequest(result.Error)
        };
    }
}

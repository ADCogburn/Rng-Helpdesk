using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RngHelpdesk.Api.DTOs;
using RngHelpdesk.Api.Helpers;
using RngHelpdesk.Api.Security;
using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Users.Commands;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Operations.Users.RunescapeAccounts;

namespace RngHelpdesk.Api.Controllers;

[Authorize(Policy = AuthPolicies.AdminPlus)]
[ApiController]
[Route("users/{userId:ulong}/runescape-accounts")]
public class RunescapeAccountsController(
    GetRunescapeAccountHandler getRunescapeHandler,
    GetPreviousRunescapeAccountsHandler getPreviousRunescapeAccountsHandler,
    GetRunescapeAccountHistoryHandler getRunescapeAccountHistoryHandler,
    LinkRunescapeAccountHandler linkRunescapeAccountHandler,
    DelinkRunescapeAccountHandler delinkRunescapeHandler,
    RenameRunescapeAccountHandler renameRunescapeHandler) : ControllerBase
{
    /// <summary>
    /// Returns only the Runescape accounts for a certain user.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpGet]
    public IActionResult GetRunescapeAccounts(ulong userId)
    {
        var query = new GetRunescapeAccountsQuery()
        {
            UserId = userId
        };

        var result = getRunescapeHandler.Handle(query);
        return Ok(result);
    }

    /// <summary>
    /// Gets the full list of previously user RSNs by a user.
    /// </summary>
    [HttpGet("previous")]
    public ActionResult<GetRunescapeAccountsResponse> GetPreviousRunescapeAccountUsernames(ulong userId)
    {
        var result = getPreviousRunescapeAccountsHandler.Handle(userId);

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
    public IActionResult LinkRunescapeAccount(ulong userId, [FromBody] string username)
    {
        var request = new LinkRunescapeAccountRequest(
            ActingUserId: User.GetUserId(),
            UserId: userId,
            Username: username);

        var result = linkRunescapeAccountHandler.Handle(request);

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
    public IActionResult DelinkRunescapeAccount(ulong userId, [FromBody] string username)
    {
        var request = new DelinkRunescapeAccountRequest(
            ActingUserId: User.GetUserId(),
            UserId: userId,
            Username: username);

        var result = delinkRunescapeHandler.Handle(request);

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
    public IActionResult RenameRunescapeAccount(ulong userId, [FromBody] RenameRunescapeAccountDto body)
    {
        var request = new RenameRunescapeAccountRequest(
            ActingUserId: User.GetUserId(),
            UserId: userId,
            OldUsername: body.OldUsername,
            NewUsername: body.NewUsername);

        var result = renameRunescapeHandler.Handle(request);

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
    public ActionResult<GetRunescapeAccountHistoryResponse> GetRunescapeAccountHistory(ulong userId)
    {
        var result = getRunescapeAccountHistoryHandler.Handle(userId);

        return result.Status switch
        {
            ResultStatus.NotFound => NotFound(result.Error),
            ResultStatus.Success => Ok(result.Value),
            _ => BadRequest(result.Error)
        };
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RngHelpdesk.Api.Security;
using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Operations.Users;

namespace RngHelpdesk.Api.Controllers;

[Authorize]
[ApiController]
[Route("users")]
public sealed class UsersController : ControllerBase
{
    private readonly IRequestContext _requestContext;

    private readonly GetUserHandler _getUserHandler;
    private readonly GetRunescapeAccountHandler _getRunescapeHandler;
    private readonly LinkRunescapeAccountHandler _linkRunescapeHandler;
    private readonly LinkDiscordAccountHandler _linkDiscordHandler;

    public UsersController(
        IRequestContextAccessor requestContext,
        GetUserHandler getUserHandler,
        GetRunescapeAccountHandler getRunescapeHandler,
        LinkRunescapeAccountHandler linkRunescapeAccountHandler,
        LinkDiscordAccountHandler linkDiscordAccountHanlder)
    {
        this._requestContext = requestContext.Context;
        this._getUserHandler = getUserHandler;
        this._getRunescapeHandler = getRunescapeHandler;
        this._linkRunescapeHandler = linkRunescapeAccountHandler;
        this._linkDiscordHandler = linkDiscordAccountHanlder;
    }

    /// <summary>
    /// Returns a user and all of their linked accounts.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:int}")]
    public ActionResult<GetUserResponse> GetUser(int id)
    {
        var response = _getUserHandler.Handle(_requestContext, id);
        return Ok(response);
    }

    /// <summary>
    /// Returns only the Runescape accounts for a certain user.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:int}/runescape-accounts")]
    public IActionResult GetRunescapeAccount(int id)
    {
        var response = _getRunescapeHandler.Handle(_requestContext, id);
        return Ok(response);
    }

    /// <summary>
    /// Adds a Runescape account username to a users list of active accounts.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("{id:int}/runescape-accounts")]
    public IActionResult LinkRunescapeAccount(int id, [FromBody] string username)
    {
        var request = new LinkRunescapeAccountRequest()
        {
            UserId = id,
            Username = username
        };

        _linkRunescapeHandler.Handle(_requestContext, request);
        return NoContent();
    }

    /// <summary>
    /// Adds a Discord account to a users list of active accounts.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("{id:int}/discord-accounts")]
    public IActionResult LinkDiscordAccount(int id, [FromBody] ulong discordId)
    {
        var request = new LinkDiscordAccountRequest()
        {
            UserId = id,
            DiscordId = discordId
        };

        _linkDiscordHandler.Handle(_requestContext, request);
        return NoContent();
    }
}
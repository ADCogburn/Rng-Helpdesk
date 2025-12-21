using Microsoft.AspNetCore.Mvc;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Operations.Handlers.Users;

namespace RngHelpdesk.Api.Controllers;

[ApiController]
[Route("users")]
public sealed class UsersController : ControllerBase
{
    private readonly GetUserHandler _getUserHandler;
    private readonly GetRunescapeAccountHandler _getRunescapeHandler;
    private readonly LinkRunescapeAccountHandler _linkRunescapeHandler;
    private readonly LinkDiscordAccountHandler _linkDiscordHandler;

    public UsersController(
        GetUserHandler _getUserHandler,
        GetRunescapeAccountHandler _getRunescapeHandler,
        LinkRunescapeAccountHandler _linkRunescapeAccountHandler,
        LinkDiscordAccountHandler _linkDiscordAccountHanlder)
    {
        this._getUserHandler = _getUserHandler;
        this._getRunescapeHandler = _getRunescapeHandler;
        this._linkRunescapeHandler = _linkRunescapeAccountHandler;
        this._linkDiscordHandler = _linkDiscordAccountHanlder;
    }

    [HttpGet("{id:int}")]
    public ActionResult<GetUserResponse> GetUser(int id)
    {
        var response = _getUserHandler.Handle(id);
        return Ok(response);
    }

    [HttpGet("{id:int}/runescape-accounts")]
    public IActionResult GetRunescapeAccount(int id)
    {
        var response = _getRunescapeHandler.Handle(id);
        return Ok(response);
    }

    [HttpPost("{id:int}/runescape-accounts")]
    public IActionResult LinkRunescapeAccount(int id, LinkRunescapeAccountRequest request)
    {
        _linkRunescapeHandler.Handle(id, request.Username);
        return NoContent();
    }

    [HttpPost("{id:int}/discord-accounts")]
    public IActionResult LinkDiscordAccount(int id, LinkDiscordAccountRequest request)
    {
        _linkDiscordHandler.Handle(id, request.DiscordId);
        return NoContent();
    }
}
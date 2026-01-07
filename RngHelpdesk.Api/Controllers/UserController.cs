using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RngHelpdesk.Api.DTOs;
using RngHelpdesk.Api.Security;
using RngHelpdesk.Contracts.Points.Commands;
using RngHelpdesk.Contracts.Points.Queries;
using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Operations.Points;
using RngHelpdesk.Operations.Users;
using RngHelpdesk.Operations.Users.DiscordAccounts;
using RngHelpdesk.Operations.Users.RunescapeAccounts;

namespace RngHelpdesk.Api.Controllers;

[Authorize]
[ApiController]
[Route("users")]
public sealed class UsersController : ControllerBase
{
    private readonly IRequestContext _requestContext;

    private readonly GetAllUsersHandler _getAllUsersHandler;
    private readonly GetUserHandler _getUserHandler;
    private readonly GetRunescapeAccountHandler _getRunescapeHandler;
    private readonly LinkRunescapeAccountHandler _linkRunescapeHandler;
    private readonly LinkDiscordAccountHandler _linkDiscordHandler;
    private readonly AddPointsToUserHandler _addPointsHandler;
    private readonly RemovePointsFromUserHandler _removePointsHandler;
    private readonly GetPointHistoryForUserHandler _getPointHistoryHandler;

    public UsersController(
        IRequestContextAccessor requestContext,
        GetAllUsersHandler getAllUsersHandler,
        GetUserHandler getUserHandler,
        GetRunescapeAccountHandler getRunescapeHandler,
        LinkRunescapeAccountHandler linkRunescapeAccountHandler,
        LinkDiscordAccountHandler linkDiscordAccountHanlder,
        AddPointsToUserHandler addPointsHandler,
        RemovePointsFromUserHandler removePointsFromUserHandler,
        GetPointHistoryForUserHandler getPointHistoryHandler)
    {
        this._requestContext = requestContext.Context;
        this._getAllUsersHandler = getAllUsersHandler;
        this._getUserHandler = getUserHandler;
        this._getRunescapeHandler = getRunescapeHandler;
        this._linkRunescapeHandler = linkRunescapeAccountHandler;
        this._linkDiscordHandler = linkDiscordAccountHanlder;
        this._addPointsHandler = addPointsHandler;
        this._removePointsHandler = removePointsFromUserHandler;
        this._getPointHistoryHandler = getPointHistoryHandler;
    }

    /// <summary>
    /// Returns all users and a count of total users.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public ActionResult<GetAllUsersResponse> GetAllUsers()
    {
        var response = _getAllUsersHandler.Handle(_requestContext);
        return Ok(response);
    }

    /// <summary>
    /// Returns a user and all of their linked accounts.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:int}")]
    public ActionResult<GetUserResponse> GetUser(int id)
    {
        var query = new GetUserByIdQuery(id);

        var response = _getUserHandler.Handle(_requestContext, query);
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
        var query = new GetRunescapeAccountsQuery()
        {
            UserId = id
        };

        var response = _getRunescapeHandler.Handle(_requestContext, query);
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

    /// <summary>
    /// Adds points to a user.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="points"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    [HttpPost("{id:int}/points/add")]
    public IActionResult AddPoints(int id, [FromBody] AddPointsDto request)
    {
        var command = new AddPointsToUserRequest
        {
            UserId = id,
            Points = request.Points,
            Reason = request.Reason
        };

        _addPointsHandler.Handle(_requestContext, command);
        return NoContent();
    }

    /// <summary>
    /// Removes points from a user.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="points"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    [HttpPost("{id:int}/points/remove")]
    public IActionResult RemovePoints(int id, [FromBody] RemovePointsDto request)
    {
        var command = new RemovePointsFromUserRequest
        {
            UserId = id,
            Points = request.Points,
            Reason = request.Reason
        };

        _removePointsHandler.Handle(_requestContext, command);
        return NoContent();
    }

    /// <summary>
    /// Gets the point history for a user.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:int}/point-history")]
    public ActionResult<GetPointHistoryForUserResponse> GetPointHistoryForUser(int id)
    {
        throw new NotImplementedException();
        //var response = _getPointHistoryHandler.Handle(_requestContext, id);
        //return Ok(response);
    }
}
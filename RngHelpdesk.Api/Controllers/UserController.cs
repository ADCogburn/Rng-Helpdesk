using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RngHelpdesk.Api.DTOs;
using RngHelpdesk.Api.Helpers;
using RngHelpdesk.Api.Security;
using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Points.Commands;
using RngHelpdesk.Contracts.Points.Queries;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Operations.Points;
using RngHelpdesk.Operations.Users;

namespace RngHelpdesk.Api.Controllers;

[Authorize(Policy = AuthPolicies.AdminPlus)]
[ApiController]
[Route("[controller]")]
public sealed class UsersController(
    GetAllUsersHandler getAllUsersHandler,
    GetUserHandler getUserHandler,
    GetUsersHandler getUsersHandler,
    GetUserLifecycleHistoryHandler getUserLifecycleHistoryHandler,
    AddPointsToUserHandler addPointsHandler,
    RemovePointsFromUserHandler removePointsFromUserHandler,
    GetPointHistoryForUserHandler getPointHistoryHandler) : ControllerBase
{
    /// <summary>
    /// Returns all users and a count of total users.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public ActionResult<GetAllUsersResponse> GetAllUsers()
    {
        var result = getAllUsersHandler.Handle();
        return Ok(result.Value);
    }

    /// <summary>
    /// Returns a user and all of their linked accounts.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}")]
    public ActionResult<GetUserResponse> GetUserById(ulong id)
    {
        var query = new GetUserByIdQuery(id);

        var result = getUserHandler.Handle(query);

        if (!result.Success)
            return NotFound(result.Error);

        return Ok(result.Value);
    }

    /// <summary>
    /// Returns an active user (fetched via RSN) and all of their linked accounts.
    /// </summary>
    /// <param name="rsn"></param>
    /// <returns></returns>
    [HttpGet("by-rsn/{rsn}")]
    public ActionResult<GetUserResponse> GetUserByRsn(string rsn)
    {
        var query = new GetUserByRunescapeUsernameQuery(rsn);

        var result = getUserHandler.Handle(query);

        if (!result.Success)
            return NotFound(result.Error);

        return Ok(result.Value);
    }

    /// <summary>
    /// Returns all users who used a particular RSN before.
    /// </summary>
    /// <param name="previousRsn"></param>
    /// <returns></returns>
    [HttpGet("by-historical-rsn/{previousRsn}")]
    public ActionResult<GetUsersResponse> GetUsersByPreviousRsn(string previousRsn)
    {
        var query = new GetUsersByHistoricalRunescapeUsernameQuery(previousRsn);

        var result = getUsersHandler.Handle(query);

        if (!result.Success)
            return NotFound(result.Error);

        return Ok(result.Value);
    }

    /// <summary>
    /// Returns the lifecycle history of a user.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}/lifecycle")]
    public ActionResult<GetUserLifecycleHistoryResponse> GetUserLifecycle(ulong id)
    {
        var query = new GetUserLifecycleHistoryQuery()
        {
            UserId = id
        };

        var result = getUserLifecycleHistoryHandler.Handle(query);

        if (!result.Success)
            return NotFound(result.Error);

        return Ok(result.Value);
    }

    // TODO: Get admin lifecycle (history of admin/deadmin)
    // TODO: Consider if any of these could be non-admin commands.

    /// <summary>
    /// Adds points to a user.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("{id:long}/points/add")]
    public IActionResult AddPoints(ulong id, [FromBody] AddPointsDto request)
    {
        var command = new AddPointsToUserRequest
        {
            ActingUserId = User.GetUserId(),
            UserId = id,
            Points = request.Points,
            Reason = request.Reason
        };

        var result = addPointsHandler.Handle(command);

        return result.Status switch
        {
            ResultStatus.NotFound => NotFound(result.Error),
            ResultStatus.Success => NoContent(),
            _ => BadRequest(result.Error)
        };
    }

    /// <summary>
    /// Removes points from a user.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("{id:long}/points/remove")]
    public IActionResult RemovePoints(ulong id, [FromBody] RemovePointsDto request)
    {
        var command = new RemovePointsFromUserRequest
        {
            ActingUserId = User.GetUserId(),
            UserId = id,
            Points = request.Points,
            Reason = request.Reason
        };

        var result = removePointsFromUserHandler.Handle(command);


        return result.Status switch
        {
            ResultStatus.NotFound => NotFound(result.Error),
            ResultStatus.Success => NoContent(),
            _ => BadRequest(result.Error)
        };
    }

    /// <summary>
    /// Gets the point history for a user.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:long}/point-history")]
    public ActionResult<GetPointHistoryForUserResponse> GetPointHistoryForUser(ulong id)
    {
        var query = new GetPointHistoryForUserQuery()
        {
            UserId = id
        };

        var result = getPointHistoryHandler.Handle(query);

        if (!result.Success)
            return NotFound(result.Error);

        return Ok(result.Value);
    }
}
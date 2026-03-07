using Microsoft.AspNetCore.Mvc;
using RngHelpdesk.Api.Controllers;
using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Contracts.Users.Commands;
using RngHelpdesk.Domain.Users;
using RngHelpdesk.Operations.Admin;

[ApiController]
[Route("admin/users")]
public sealed class AdminController : ControllerBase
{
    private readonly IRequestContextAccessor _requestContextAccessor;
    private readonly ChangeAdminStatusHandler _changeAdminStatusHandler;
    private readonly CreateUserHandler _createUserHandler;

    public AdminController(
        IRequestContextAccessor requestContextAccessor,
        ChangeAdminStatusHandler changeAdminStatusHandler,
        CreateUserHandler createUserHandler)
    {
        _requestContextAccessor = requestContextAccessor;
        _changeAdminStatusHandler = changeAdminStatusHandler;
        _createUserHandler = createUserHandler;
    }

    /// <summary>
    /// Creates a new user and generates temporary login credentials.
    /// </summary>
    [HttpPost("create")]
    public ActionResult<CreateUserResponse> CreateUser([FromBody] CreateUserRequest request)
    {
        var result = _createUserHandler.Handle(_requestContextAccessor.Context, request);

        if (!result.Success)
            return BadRequest(result.Error);

        return CreatedAtAction(
            nameof(UsersController.GetUser),
            "Users",
            new { id = result.Value!.UserId },
            result.Value);
    }

    /// <summary>
    /// Promotes a user to Administrator.
    /// </summary>
    [HttpPost("{id:int}")]
    public IActionResult AdminUser(int id)
    {
        var requestContext = _requestContextAccessor.Context;

        var request = new ChangeAdminStatusRequest
        {
            UserId = id,
            NewRole = AuthorityRole.Administrator
        };

        _changeAdminStatusHandler.Handle(requestContext, request);
        return NoContent();
    }

    /// <summary>
    /// Removes administrative privileges from a user.
    /// </summary>
    [HttpDelete("{id:int}")]
    public IActionResult DeAdminUser(int id)
    {
        var requestContext = _requestContextAccessor.Context;

        var request = new ChangeAdminStatusRequest
        {
            UserId = id,
            NewRole = AuthorityRole.Member
        };

        _changeAdminStatusHandler.Handle(requestContext, request);
        return NoContent();
    }
}

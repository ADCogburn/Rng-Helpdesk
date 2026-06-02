using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RngHelpdesk.Api.Controllers;
using RngHelpdesk.Api.Security;
using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Contracts.Users.Commands;
using RngHelpdesk.Operations.Admin;

[ApiController]
[Authorize(Policy = AuthPolicies.AdminPlus)]
[Route("admin/users")]
public sealed class AdminController : ControllerBase
{
    private readonly IRequestContextAccessor _requestContextAccessor;
    private readonly ChangeUserRoleHandler _changeUserRoleHandler;
    private readonly CreateUserHandler _createUserHandler;

    public AdminController(
        IRequestContextAccessor requestContextAccessor,
        ChangeUserRoleHandler changeUserRoleHandler,
        CreateUserHandler createUserHandler)
    {
        _requestContextAccessor = requestContextAccessor;
        _changeUserRoleHandler = changeUserRoleHandler;
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

        var request = new ChangeUserRoleRequest
        {
            UserId = id,
            NewRole = AppRole.Administrator
        };

        _changeUserRoleHandler.Handle(requestContext, request);
        return NoContent();
    }

    /// <summary>
    /// Removes administrative privileges from a user.
    /// </summary>
    [HttpDelete("{id:int}")]
    public IActionResult DeAdminUser(int id)
    {
        var requestContext = _requestContextAccessor.Context;

        var request = new ChangeUserRoleRequest
        {
            UserId = id,
            NewRole = AppRole.Member
        };

        _changeUserRoleHandler.Handle(requestContext, request);
        return NoContent();
    }
}

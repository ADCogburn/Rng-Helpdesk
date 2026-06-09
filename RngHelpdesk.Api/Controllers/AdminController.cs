using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RngHelpdesk.Api.Helpers;
using RngHelpdesk.Api.Security;
using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Contracts.Users.Commands;
using RngHelpdesk.Operations.Admin;

namespace RngHelpdesk.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthPolicies.AdminPlus)]
[Route("[controller]/users")]
public sealed class AdminController(
                        ChangeUserRoleHandler changeUserRoleHandler,
                        CreateUserHandler createUserHandler) : ControllerBase
{
    private readonly ChangeUserRoleHandler _changeUserRoleHandler = changeUserRoleHandler;
    private readonly CreateUserHandler _createUserHandler = createUserHandler;


    /// <summary>
    /// Creates a new user and generates temporary login credentials.
    /// </summary>
    [HttpPost("create")]
    public ActionResult<CreateUserResponse> CreateUser([FromBody] CreateUserRequest request)
    {
        var result = _createUserHandler.Handle(User.GetUserId(), request);

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
    [HttpPost("{id:ulong}/promote")]
    public async Task<IActionResult> AdminUser(ulong id)
    {
        var request = new ChangeUserRoleCommand
        (
            ActingUserId: User.GetUserId(),
            TargetUserId: id,
            NewRole: AppRole.Administrator
        );

        await _changeUserRoleHandler.Handle(request);

        return NoContent();
    }

    /// <summary>
    /// Removes administrative privileges from a user.
    /// </summary>
    [HttpPost("{id:ulong}/demote")]
    public async Task<IActionResult> DeAdminUser(ulong id)
    {
        var request = new ChangeUserRoleCommand
        (
            ActingUserId: User.GetUserId(),
            TargetUserId: id,
            NewRole: AppRole.Member
        );

        await _changeUserRoleHandler.Handle(request);

        return NoContent();
    }
}
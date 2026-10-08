using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RngHelpdesk.Api.Helpers;
using RngHelpdesk.Api.Security;
using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Contracts.Users.Commands;
using RngHelpdesk.Operations.Common;

namespace RngHelpdesk.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthPolicies.AdminPlus)]
[Route("[controller]")]
public sealed class AdminController(
                        ICommandHandler<ChangeUserRoleCommand> changeUserRoleHandler,
                        ICommandHandler<CreateUserRequest, CreateUserResponse> createUserHandler,
                        ICommandHandler<DeactivateUserRequest> deactivateUserHandler,
                        ICommandHandler<ReactivateUserRequest> reactivateUserHandler) : ControllerBase
{
    /// <summary>
    /// Creates a new user and generates temporary login credentials.
    /// </summary>
    [HttpPost("create")]
    public async Task<ActionResult<CreateUserResponse>> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        request.ActingUserId = User.GetUserId();

        var result = await createUserHandler.Handle(request, cancellationToken);

        if (!result.Success)
            return BadRequest(result.Error);

        return CreatedAtAction(
            nameof(UsersController.GetUserById),
            "Users",
            new { id = result.Value!.UserId },
            result.Value);
    }

    /// <summary>
    /// Promotes a user to Administrator.
    /// </summary>
    [HttpPost("{id:long}/promote")]
    public async Task<IActionResult> AdminUser(ulong id, CancellationToken cancellationToken)
    {
        var request = new ChangeUserRoleCommand
        (
            ActingUserId: User.GetUserId(),
            TargetUserId: id,
            NewRole: AppRole.Administrator
        );

        var result = await changeUserRoleHandler.Handle(request, cancellationToken);

        if (!result.Success)
            return BadRequest(result.Error);

        return NoContent();
    }

    /// <summary>
    /// Removes administrative privileges from a user.
    /// </summary>
    [HttpPost("{id:long}/demote")]
    public async Task<IActionResult> DeAdminUser(ulong id, CancellationToken cancellationToken)
    {
        var request = new ChangeUserRoleCommand
        (
            ActingUserId: User.GetUserId(),
            TargetUserId: id,
            NewRole: AppRole.Member
        );

        var result = await changeUserRoleHandler.Handle(request, cancellationToken);

        if (!result.Success)
            return BadRequest(result.Error);

        return NoContent();
    }

    /// <summary>
    /// Deactivates a clan member.
    /// </summary>
    [HttpPost("{id:long}/deactivate")]
    public async Task<IActionResult> DeactivateUser(ulong id, CancellationToken cancellationToken)
    {
        var request = new DeactivateUserRequest(User.GetUserId(), id);

        var result = await deactivateUserHandler.Handle(request, cancellationToken);

        if (!result.Success)
            return BadRequest(result.Error);

        return NoContent();
    }

    /// <summary>
    /// Reactivates a previously deactivated clan member.
    /// </summary>
    [HttpPost("{id:long}/reactivate")]
    public async Task<IActionResult> ReactivateUser(ulong id, CancellationToken cancellationToken)
    {
        var request = new ReactivateUserRequest(User.GetUserId(), id);

        var result = await reactivateUserHandler.Handle(request, cancellationToken);

        if (!result.Success)
            return BadRequest(result.Error);

        return NoContent();
    }
}
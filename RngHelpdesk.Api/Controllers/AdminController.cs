using Microsoft.AspNetCore.Mvc;
using RngHelpdesk.Api.Security;
using RngHelpdesk.Contracts.Users.Commands;
using RngHelpdesk.Domain.Users;
using RngHelpdesk.Operations.Users;

[ApiController]
[Route("admin/users")]
public sealed class AdminController : ControllerBase
{
    private readonly IRequestContextAccessor _requestContextAccessor;
    private readonly ChangeAdminStatusHandler _changeAdminStatusHandler;

    public AdminController(
        IRequestContextAccessor requestContextAccessor,
        ChangeAdminStatusHandler changeAdminStatusHandler)
    {
        _requestContextAccessor = requestContextAccessor;
        _changeAdminStatusHandler = changeAdminStatusHandler;
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

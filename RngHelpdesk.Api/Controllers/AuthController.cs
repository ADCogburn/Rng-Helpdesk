using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RngHelpdesk.Api.Helpers;
using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Infrastructure.Security;
using RngHelpdesk.Infrastructure.Users;
using RngHelpdesk.Operations.Helpers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RngHelpdesk.Api.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController(
    ICredentialStore credentialStore,
    IConfiguration config,
    IUserSummaryReadStore userSummaryReadStore) : ControllerBase
{
    [Authorize]
    [HttpGet("me")]
    public ActionResult<GetUserResponse> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!ulong.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        if (!userSummaryReadStore.TryGetById(userId, out var user) || user is null)
            return BadRequest("User not found - contact an administrator.");

        return Ok(new GetUserResponse(user.ToDto()));
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var authenticatedUser = credentialStore.ValidateCredentials(
            request.Username,
            request.Password);

        if (authenticatedUser is null)
            return Unauthorized();

        if (!userSummaryReadStore.TryGetById(authenticatedUser.UserId, out var user) || user is null)
            return BadRequest("User not found - contact an administrator.");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, authenticatedUser.UserId.ToString()),
            new Claim(ClaimTypes.Role, user.AppRole.ToString())

            // Later:
            // Discord Bot flow should also issue this same normal user JWT after validating the Discord snowflake through a bot-only endpoint.
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(config["Jwt:Key"]!)
        );

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            )
        );

        return Ok(new LoginResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token)
        });
    }

    [Authorize]
    [HttpPost("change-username")]
    public IActionResult ChangeUsername([FromBody] ChangeUsernameRequest request)
    {
        var userId = User.GetUserId();

        if (string.IsNullOrWhiteSpace(request.NewUsername))
            return BadRequest("Username is required.");

        credentialStore.ChangeUsername(userId, request.NewUsername);

        return NoContent();
    }

    // TODO: Change password

}
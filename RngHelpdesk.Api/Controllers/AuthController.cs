using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RngHelpdesk.Api.DTOs;
using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Infrastructure.Security;
using RngHelpdesk.Infrastructure.Users;
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
    public async Task<ActionResult<GetUserResponse>> GetCurrentUser(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!ulong.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var user = await userSummaryReadStore.GetByIdAsync(userId, cancellationToken);

        if (user is null)
            return BadRequest("User not found - contact an administrator.");

        return Ok(new GetUserResponse
        (
            Id: user.UserId,
            AppRole: user.AppRole,
            ClanPoints: user.ClanPoints,
            Rank: user.Rank,
            IsActive: user.IsActive,
            DateCreated: user.DateCreated,
            DiscordAccount: user.DiscordAccount,
            RunescapeAccounts: user.RunescapeAccounts.ToList()
        ));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var authenticatedUser = await credentialStore.ValidateCredentialsAsync(
            request.Username,
            request.Password,
            cancellationToken);

        if (authenticatedUser is null)
            return Unauthorized();

        var user = await userSummaryReadStore.GetByIdAsync(authenticatedUser.UserId, cancellationToken);

        if (user is null)
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
}
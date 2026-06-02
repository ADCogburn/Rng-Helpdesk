using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Infrastructure.Security;
using RngHelpdesk.Infrastructure.Users;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RngHelpdesk.Api.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthStore _authStore;
    private readonly IConfiguration _config;
    private readonly UserSummaryProjection _users;


    public AuthController(
        IAuthStore authStore,
        IConfiguration config,
        UserSummaryProjection users)
    {
        _authStore = authStore;
        _config = config;
        _users = users;
    }

    /// <summary>
    /// Returns the current authenticated user's info including authorization role.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var user = _users.GetSingleById(userId);

        return Ok(new
        {
            userId = user.UserId,
            role = User.FindFirst(ClaimTypes.Role)?.Value,

            discordAccounts = user.DiscordAccounts,
            runescapeAccounts = user.RunescapeAccounts,

            currentPoints = user.CurrentClanPoints,
            rank = user.Rank
        });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var authenticatedUser = _authStore.ValidateCredentials(
            request.Username,
            request.Password);
        if (authenticatedUser is null)
            return Unauthorized();

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, authenticatedUser.UserId.ToString()),
            new Claim(ClaimTypes.Role, authenticatedUser.Role.ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)
        );

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            )
        );

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token)
        });
    }
}
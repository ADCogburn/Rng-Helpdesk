using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RngHelpdesk.Api.DTOs;
using RngHelpdesk.Contracts.Common.Ranks;
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
public sealed class AuthController : ControllerBase
{
    private readonly ICredentialStore _authStore;
    private readonly IConfiguration _config;
    private readonly RankResolver _rankResolver;
    private readonly UserSummaryProjection _users;

    public AuthController(
        ICredentialStore authStore,
        IConfiguration config,
        RankResolver rankResolver,
        UserSummaryProjection users)
    {
        _authStore = authStore;
        _config = config;
        _rankResolver = rankResolver;
        _users = users;
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult<GetUserResponse> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var user = _users.GetSingleById(userId);

        var roleText = User.FindFirst(ClaimTypes.Role)?.Value;

        var appRole = Enum.TryParse<AppRole>(roleText, out var parsedRole)
            ? parsedRole
            : AppRole.Member;

        var rank = _rankResolver.Resolve(appRole, user.ClanPoints);

        return Ok(new GetUserResponse
        {
            Id = user.UserId,
            AppRole = appRole,
            ClanPoints = user.ClanPoints,
            Rank = rank.ToString(),
            IsActive = user.IsActive,
            DateCreated = user.DateCreated,
            DiscordAccounts = user.DiscordAccount.ToList(),
            RunescapeAccounts = user.RunescapeAccounts.ToList()
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

            // Later:
            // Discord Bot flow should also issue this same normal user JWT after validating the Discord snowflake through a bot-only endpoint.
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

        return Ok(new LoginResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token)
        });
    }
}
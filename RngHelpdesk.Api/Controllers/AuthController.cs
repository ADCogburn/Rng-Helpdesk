using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Infrastructure.Security;
using RngHelpdesk.Infrastructure.Users;
using RngHelpdesk.Operations.Security;
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
    private readonly IRequestContextAccessor _requestContextAccessor;
    private readonly IActorUserResolver _actorUserResolver;
    private readonly UserSummaryProjection _users;

    public AuthController(
        IAuthStore authStore,
        IConfiguration config,
        IRequestContextAccessor requestContextAccessor,
        IActorUserResolver actorUserResolver,
        UserSummaryProjection users)
    {
        _authStore = authStore;
        _config = config;
        _requestContextAccessor = requestContextAccessor;
        _actorUserResolver = actorUserResolver;
        _users = users;
    }

    /// <summary>
    /// Returns the current authenticated user's info including authorization role.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        var ctx = _requestContextAccessor.Context;

        if (!ctx.IsAuthenticated)
            return Unauthorized();

        var userId = _actorUserResolver.ResolveUserId(ctx.ActorId, ctx.ActorType);
        if (userId is null)
            return NotFound(new { error = "Actor not linked to a user." });

        var user = _users.GetSingleById(userId.Value);

        return Ok(new
        {
            userId = user.UserId,
            actorId = ctx.ActorId,
            actorType = ctx.ActorType.ToString(),
            authorityRole = user.AuthorityRole.ToString()
        });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var actorId = _authStore.ValidateCredentials(
            request.Username,
            request.Password);

        if (actorId is null)
            return Unauthorized();

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, actorId.Value.ToString()),
            new Claim("actor_type", ActorType.WebUser.ToString())
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
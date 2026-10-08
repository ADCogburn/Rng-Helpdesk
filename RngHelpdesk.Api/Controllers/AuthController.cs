using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RngHelpdesk.Api.DTOs;
using RngHelpdesk.Api.Security;
using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Points.Queries;
using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Infrastructure.Security;
using RngHelpdesk.Infrastructure.Users;
using RngHelpdesk.Operations.Common;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace RngHelpdesk.Api.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController(
    ICredentialStore credentialStore,
    IConfiguration config,
    IUserSummaryReadStore userSummaryReadStore,
    JwtTokenIssuer tokenIssuer,
    IQueryHandler<GetPointHistoryForUserQuery, GetPointHistoryForUserResponse> getPointHistoryHandler) : ControllerBase
{
    [Authorize]
    [HttpGet("me/point-history")]
    public async Task<ActionResult<GetPointHistoryForUserResponse>> GetMyPointHistory(CancellationToken cancellationToken)
    {
        if (!ulong.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
            return Unauthorized();

        var result = await getPointHistoryHandler.Handle(new GetPointHistoryForUserQuery { UserId = userId }, cancellationToken);

        if (!result.Success)
            return NotFound(result.Error);

        return Ok(result.Value);
    }

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
            new Claim(ClaimTypes.Name, authenticatedUser.Username),
            new Claim(ClaimTypes.Role, user.AppRole.ToString())
        };

        var issued = tokenIssuer.Issue(claims, JwtTokenIssuer.LoginLifetime);

        return Ok(new LoginResponse
        {
            Token = issued.Token,
            MustChangePassword = authenticatedUser.MustChangePassword
        });
    }

    /// <summary>
    /// Step 1 of the Discord bot flow: trades the shared secret (<c>DiscordBot:ApiKey</c>) for a
    /// bot-only JWT. Disabled (always 401) while the key is unset/blank.
    /// </summary>
    [HttpPost("bot/token")]
    public IActionResult BotToken([FromBody] BotTokenRequest request)
    {
        var configuredKey = config["DiscordBot:ApiKey"];

        if (string.IsNullOrWhiteSpace(configuredKey))
            return Unauthorized();

        var expected = Encoding.UTF8.GetBytes(configuredKey);
        var provided = Encoding.UTF8.GetBytes(request.ApiKey ?? string.Empty);

        if (!CryptographicOperations.FixedTimeEquals(expected, provided))
            return Unauthorized();

        var issued = tokenIssuer.IssueBotToken();

        return Ok(new BotTokenResponse { Token = issued.Token, ExpiresAt = issued.ExpiresAt });
    }

    /// <summary>
    /// Step 2 of the Discord bot flow: trades the bot JWT plus a Discord snowflake for a short-lived
    /// normal user JWT. Deliberately omits <c>ClaimTypes.Name</c> so <c>change-password</c> rejects it.
    /// </summary>
    [Authorize(Policy = AuthPolicies.DiscordBotOnly)]
    [HttpPost("discord")]
    public async Task<IActionResult> DiscordExchange([FromBody] DiscordExchangeRequest request, CancellationToken cancellationToken)
    {
        var user = await userSummaryReadStore.GetByIdAsync(request.DiscordId, cancellationToken);

        if (user is null)
            return NotFound("No helpdesk user is registered for that Discord id.");

        if (!user.IsActive)
            return StatusCode(StatusCodes.Status403Forbidden);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Role, user.AppRole.ToString())
        };

        var issued = tokenIssuer.Issue(claims, JwtTokenIssuer.DiscordUserTokenLifetime);

        return Ok(new DiscordExchangeResponse
        {
            Token = issued.Token,
            ExpiresAt = issued.ExpiresAt,
            AppRole = user.AppRole
        });
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request, CancellationToken cancellationToken)
    {
        var username = User.FindFirst(ClaimTypes.Name)?.Value;

        if (!ulong.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId) || string.IsNullOrEmpty(username))
            return Unauthorized();

        var authenticatedUser = await credentialStore.ValidateCredentialsAsync(
            username,
            request.CurrentPassword,
            cancellationToken);

        if (authenticatedUser is null || authenticatedUser.UserId != userId)
            return BadRequest("Current password is incorrect.");

        await credentialStore.ChangePasswordAsync(userId, request.NewPassword, cancellationToken);

        return NoContent();
    }
}
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RngHelpdesk.Api.Security;

/// <summary>
/// Builds and signs the JWTs issued by <c>AuthController</c> (login, bot token, Discord exchange),
/// reading <c>Jwt:*</c> configuration.
/// </summary>
public sealed class JwtTokenIssuer(IConfiguration config)
{
    public const string ClientTypeClaim = "client_type";
    public const string DiscordBotClientType = "discord_bot";

    public static readonly TimeSpan LoginLifetime = TimeSpan.FromHours(8);
    public static readonly TimeSpan BotTokenLifetime = TimeSpan.FromHours(1);
    public static readonly TimeSpan DiscordUserTokenLifetime = TimeSpan.FromMinutes(15);

    public IssuedToken Issue(IEnumerable<Claim> claims, TimeSpan lifetime)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(config["Jwt:Key"]!)
        );

        var expiresAt = DateTime.UtcNow.Add(lifetime);

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            )
        );

        return new IssuedToken(new JwtSecurityTokenHandler().WriteToken(token), new DateTimeOffset(expiresAt, TimeSpan.Zero));
    }

    /// <summary>A JWT carrying only <c>client_type=discord_bot</c> -- no identity and no role.</summary>
    public IssuedToken IssueBotToken()
        => Issue([new Claim(ClientTypeClaim, DiscordBotClientType)], BotTokenLifetime);
}

public sealed record IssuedToken(string Token, DateTimeOffset ExpiresAt);

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RngHelpdesk.Api.Controllers;
using RngHelpdesk.Api.DTOs;
using RngHelpdesk.Api.Security;
using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Contracts.Users.Commands;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Domain.Users;

namespace RngHelpdesk.Api.Tests.Controllers;

public class AuthControllerTests
{
    private readonly ApiTestFixture _fixture = new();

    private AuthController CreateController()
        => CreateController(botApiKey: null);

    private AuthController CreateController(string? botApiKey)
    {
        var config = CreateJwtConfig(botApiKey);
        return new(_fixture.CredentialStore, config, _fixture.UserSummaryProjection, new JwtTokenIssuer(config));
    }

    private static IConfiguration CreateJwtConfig(string? botApiKey = null) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-only-signing-key-at-least-32-characters-long",
                ["Jwt:Issuer"] = "RngHelpdeskTests",
                ["Jwt:Audience"] = "RngHelpdeskTests",
                ["DiscordBot:ApiKey"] = botApiKey
            })
            .Build();

    private static JwtSecurityToken ReadToken(string token) => new JwtSecurityTokenHandler().ReadJwtToken(token);

    private static void SetBotUser(ControllerBase controller)
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("client_type", "discord_bot")], "TestAuth"))
            }
        };
    }

    private static void SetAnonymousUser(ControllerBase controller)
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
        };
    }

    [Fact]
    public async Task GetCurrentUser_ClaimMatchesExistingUser_ReturnsOkWithUser()
    {
        var user = await _fixture.CreateAndDispatchUserAsync(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        var controller = CreateController();
        ControllerTestHelpers.SetActingUser(controller, user.Id);

        var result = await controller.GetCurrentUser(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<GetUserResponse>(ok.Value);
        Assert.Equal(user.Id, response.Id);
    }

    [Fact]
    public async Task GetCurrentUser_NoNameIdentifierClaim_ReturnsUnauthorized()
    {
        var controller = CreateController();
        SetAnonymousUser(controller);

        var result = await controller.GetCurrentUser(CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task GetCurrentUser_ClaimUserNotInReadStore_ReturnsBadRequest()
    {
        var controller = CreateController();
        ControllerTestHelpers.SetActingUser(controller, userId: 999);

        var result = await controller.GetCurrentUser(CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithToken()
    {
        var user = await _fixture.CreateAndDispatchUserAsync(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        await _fixture.CredentialStore.SeedCredentialsAsync(user.Id, "login-user", "correct-password");
        var controller = CreateController();

        var result = await controller.Login(new LoginRequest { Username = "login-user", Password = "correct-password" }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<LoginResponse>(ok.Value);
        Assert.False(string.IsNullOrWhiteSpace(response.Token));
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        var user = await _fixture.CreateAndDispatchUserAsync(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        await _fixture.CredentialStore.SeedCredentialsAsync(user.Id, "login-user", "correct-password");
        var controller = CreateController();

        var result = await controller.Login(new LoginRequest { Username = "login-user", Password = "wrong-password" }, CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Login_CredentialsValidButUserMissingFromReadStore_ReturnsBadRequest()
    {
        // Seeds credentials for a userId that was never dispatched into UserSummaryProjection,
        // mirroring an admin/read-store desync rather than a bad password.
        await _fixture.CredentialStore.SeedCredentialsAsync(userId: 999, "orphaned-user", "correct-password");
        var controller = CreateController();

        var result = await controller.Login(new LoginRequest { Username = "orphaned-user", Password = "correct-password" }, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Login_TemporaryCredentials_ReportsMustChangePassword()
    {
        var user = await _fixture.CreateAndDispatchUserAsync(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        var (username, password) = await _fixture.CredentialStore.CreateTemporaryCredentialsAsync(user.Id, "temp-user");
        var controller = CreateController();

        var result = await controller.Login(new LoginRequest { Username = username, Password = password }, CancellationToken.None);

        var response = Assert.IsType<LoginResponse>(Assert.IsType<OkObjectResult>(result).Value);
        Assert.True(response.MustChangePassword);
    }

    [Fact]
    public async Task Login_SeededCredentials_DoesNotReportMustChangePassword()
    {
        var user = await _fixture.CreateAndDispatchUserAsync(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        await _fixture.CredentialStore.SeedCredentialsAsync(user.Id, "login-user", "correct-password");
        var controller = CreateController();

        var result = await controller.Login(new LoginRequest { Username = "login-user", Password = "correct-password" }, CancellationToken.None);

        var response = Assert.IsType<LoginResponse>(Assert.IsType<OkObjectResult>(result).Value);
        Assert.False(response.MustChangePassword);
    }

    [Fact]
    public async Task Login_ValidCredentials_TokenCarriesIdentityNameAndRole()
    {
        var user = await _fixture.CreateAndDispatchUserAsync(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        await _fixture.CredentialStore.SeedCredentialsAsync(user.Id, "login-user", "correct-password");
        var controller = CreateController();

        var result = await controller.Login(new LoginRequest { Username = "login-user", Password = "correct-password" }, CancellationToken.None);

        var response = Assert.IsType<LoginResponse>(Assert.IsType<OkObjectResult>(result).Value);
        var jwt = ReadToken(response.Token);
        Assert.Equal(user.Id.ToString(), jwt.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value);
        Assert.Equal("login-user", jwt.Claims.Single(c => c.Type == ClaimTypes.Name).Value);
        Assert.Equal(AppRole.Member.ToString(), jwt.Claims.Single(c => c.Type == ClaimTypes.Role).Value);
    }

    [Theory]
    [InlineData(null, "anything")]
    [InlineData("", "")]
    [InlineData("   ", "   ")]
    public void BotToken_KeyNotConfigured_ReturnsUnauthorized(string? configuredKey, string providedKey)
    {
        var controller = CreateController(configuredKey);

        var result = controller.BotToken(new BotTokenRequest { ApiKey = providedKey });

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public void BotToken_WrongKey_ReturnsUnauthorized()
    {
        var controller = CreateController("the-real-key");

        var result = controller.BotToken(new BotTokenRequest { ApiKey = "the-wrong-key" });

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public void BotToken_CorrectKey_ReturnsTokenWithOnlyBotClientTypeClaim()
    {
        var controller = CreateController("the-real-key");

        var result = controller.BotToken(new BotTokenRequest { ApiKey = "the-real-key" });

        var response = Assert.IsType<BotTokenResponse>(Assert.IsType<OkObjectResult>(result).Value);
        var jwt = ReadToken(response.Token);
        Assert.Equal("discord_bot", jwt.Claims.Single(c => c.Type == "client_type").Value);
        Assert.DoesNotContain(jwt.Claims, c => c.Type == ClaimTypes.NameIdentifier);
        Assert.DoesNotContain(jwt.Claims, c => c.Type == ClaimTypes.Role);
        Assert.DoesNotContain(jwt.Claims, c => c.Type == ClaimTypes.Name);
        Assert.True(response.ExpiresAt > DateTimeOffset.UtcNow.AddMinutes(50));
        Assert.True(response.ExpiresAt <= DateTimeOffset.UtcNow.AddHours(1).AddMinutes(1));
    }

    [Fact]
    public async Task DiscordExchange_UnknownDiscordId_ReturnsNotFound()
    {
        var controller = CreateController();
        SetBotUser(controller);

        var result = await controller.DiscordExchange(new DiscordExchangeRequest { DiscordId = 999 }, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task DiscordExchange_DeactivatedUser_ReturnsForbidden()
    {
        var user = await _fixture.CreateAndDispatchUserAsync(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        await _fixture.CreateDeactivateUserHandler().Handle(new DeactivateUserRequest(TestUsers.DefaultActingUserId, user.Id), CancellationToken.None);
        var controller = CreateController();
        SetBotUser(controller);

        var result = await controller.DiscordExchange(new DiscordExchangeRequest { DiscordId = user.Id }, CancellationToken.None);

        var status = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, status.StatusCode);
    }

    [Fact]
    public async Task DiscordExchange_ActiveUser_ReturnsUserTokenWithIdentityAndRoleButNoName()
    {
        var user = await _fixture.CreateAndDispatchUserAsync(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        var controller = CreateController();
        SetBotUser(controller);

        var result = await controller.DiscordExchange(new DiscordExchangeRequest { DiscordId = user.Id }, CancellationToken.None);

        var response = Assert.IsType<DiscordExchangeResponse>(Assert.IsType<OkObjectResult>(result).Value);
        Assert.Equal(AppRole.Member, response.AppRole);
        var jwt = ReadToken(response.Token);
        Assert.Equal(user.Id.ToString(), jwt.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value);
        Assert.Equal(AppRole.Member.ToString(), jwt.Claims.Single(c => c.Type == ClaimTypes.Role).Value);
        Assert.DoesNotContain(jwt.Claims, c => c.Type == ClaimTypes.Name);
        Assert.DoesNotContain(jwt.Claims, c => c.Type == "client_type");
        Assert.True(response.ExpiresAt <= DateTimeOffset.UtcNow.AddMinutes(16));
    }

    private static void SetActingUser(ControllerBase controller, ulong userId, string? username)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId.ToString()) };
        if (username is not null)
            claims.Add(new Claim(ClaimTypes.Name, username));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")) }
        };
    }

    [Fact]
    public async Task ChangePassword_CorrectCurrentPassword_ReturnsNoContentAndClearsFlag()
    {
        var user = await _fixture.CreateAndDispatchUserAsync(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        var (username, temporaryPassword) = await _fixture.CredentialStore.CreateTemporaryCredentialsAsync(user.Id, "temp-user");
        var controller = CreateController();
        SetActingUser(controller, user.Id, username);

        var result = await controller.ChangePassword(
            new ChangePasswordDto { CurrentPassword = temporaryPassword, NewPassword = "a-new-password" },
            CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        Assert.Null(await _fixture.CredentialStore.ValidateCredentialsAsync(username, temporaryPassword));
        var after = await _fixture.CredentialStore.ValidateCredentialsAsync(username, "a-new-password");
        Assert.NotNull(after);
        Assert.False(after.MustChangePassword);
    }

    [Fact]
    public async Task ChangePassword_WrongCurrentPassword_ReturnsBadRequestAndKeepsPassword()
    {
        var user = await _fixture.CreateAndDispatchUserAsync(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        await _fixture.CredentialStore.SeedCredentialsAsync(user.Id, "login-user", "correct-password");
        var controller = CreateController();
        SetActingUser(controller, user.Id, "login-user");

        var result = await controller.ChangePassword(
            new ChangePasswordDto { CurrentPassword = "wrong-password", NewPassword = "a-new-password" },
            CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.IsType<string>(badRequest.Value);
        Assert.NotNull(await _fixture.CredentialStore.ValidateCredentialsAsync("login-user", "correct-password"));
    }

    [Fact]
    public async Task ChangePassword_MissingUsernameClaim_ReturnsUnauthorized()
    {
        var controller = CreateController();
        SetActingUser(controller, userId: 1, username: null);

        var result = await controller.ChangePassword(
            new ChangePasswordDto { CurrentPassword = "x", NewPassword = "a-new-password" },
            CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result);
    }
}
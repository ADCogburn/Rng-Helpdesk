using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RngHelpdesk.Api.Controllers;
using RngHelpdesk.Api.DTOs;
using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Domain.Users;

namespace RngHelpdesk.Api.Tests.Controllers;

public class AuthControllerTests
{
    private readonly ApiTestFixture _fixture = new();

    private AuthController CreateController()
        => new(_fixture.CredentialStore, CreateJwtConfig(), _fixture.UserSummaryProjection);

    private static IConfiguration CreateJwtConfig() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-only-signing-key-at-least-32-characters-long",
                ["Jwt:Issuer"] = "RngHelpdeskTests",
                ["Jwt:Audience"] = "RngHelpdeskTests"
            })
            .Build();

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
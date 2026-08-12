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
}

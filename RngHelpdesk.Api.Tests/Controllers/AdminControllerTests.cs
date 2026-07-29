using Microsoft.AspNetCore.Mvc;
using RngHelpdesk.Api.Controllers;
using RngHelpdesk.Contracts.Models.Users.Dtos;
using RngHelpdesk.Contracts.Security;
using RngHelpdesk.Contracts.Users.Commands;

namespace RngHelpdesk.Api.Tests.Controllers;

public class AdminControllerTests
{
    private readonly ApiTestFixture _fixture = new();

    private AdminController CreateController()
    {
        var controller = new AdminController(
            _fixture.CreateChangeUserRoleHandler(),
            _fixture.CreateCreateUserHandler());

        ControllerTestHelpers.SetActingUser(controller, TestUsers.DefaultActingUserId);
        return controller;
    }

    [Fact]
    public async Task CreateUser_ValidRequest_ReturnsCreatedAtActionWithResponse()
    {
        var controller = CreateController();
        var request = new CreateUserRequest
        {
            DiscordAccount = new DiscordAccountDto { DiscordId = TestUsers.DefaultDiscordId, Username = TestUsers.DefaultDiscordUsername }
        };

        var result = await controller.CreateUser(request, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(UsersController.GetUserById), created.ActionName);
        Assert.Equal("Users", created.ControllerName);
        var response = Assert.IsType<CreateUserResponse>(created.Value);
        Assert.Equal(TestUsers.DefaultDiscordId, response.UserId);
    }

    [Fact]
    public async Task CreateUser_ActingUserIdComesFromClaimsPrincipal()
    {
        var controller = CreateController();
        var request = new CreateUserRequest
        {
            DiscordAccount = new DiscordAccountDto { DiscordId = TestUsers.DefaultDiscordId, Username = TestUsers.DefaultDiscordUsername }
        };

        await controller.CreateUser(request, CancellationToken.None);

        Assert.Equal(TestUsers.DefaultActingUserId, request.ActingUserId);
    }

    [Fact]
    public async Task CreateUser_DiscordAccountMissing_ReturnsBadRequestWithHandlerError()
    {
        var controller = CreateController();
        var request = new CreateUserRequest { DiscordAccount = null! };

        var result = await controller.CreateUser(request, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Discord account is required.", badRequest.Value);
    }

    [Fact]
    public async Task AdminUser_TargetUserExists_ReturnsNoContentAndPromotesToAdministrator()
    {
        var user = await _fixture.CreateAndDispatchUserAsync(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        var controller = CreateController();

        var result = await controller.AdminUser(user.Id, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        var summary = await _fixture.UserSummaryProjection.GetByIdAsync(user.Id);
        Assert.NotNull(summary);
        Assert.Equal(AppRole.Administrator, summary!.AppRole);
    }

    [Fact]
    public async Task AdminUser_TargetUserDoesNotExist_ReturnsBadRequestWithHandlerError()
    {
        var controller = CreateController();

        var result = await controller.AdminUser(999, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("User not found.", badRequest.Value);
    }

    [Fact]
    public async Task DeAdminUser_TargetUserExists_ReturnsNoContentAndDemotesToMember()
    {
        var user = await _fixture.CreateAndDispatchUserAsync(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        await CreateController().AdminUser(user.Id, CancellationToken.None);
        var controller = CreateController();

        var result = await controller.DeAdminUser(user.Id, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        var summary = await _fixture.UserSummaryProjection.GetByIdAsync(user.Id);
        Assert.NotNull(summary);
        Assert.Equal(AppRole.Member, summary!.AppRole);
    }

    [Fact]
    public async Task DeAdminUser_TargetUserDoesNotExist_ReturnsBadRequestWithHandlerError()
    {
        var controller = CreateController();

        var result = await controller.DeAdminUser(999, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("User not found.", badRequest.Value);
    }
}

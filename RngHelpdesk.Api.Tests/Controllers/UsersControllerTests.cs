using Microsoft.AspNetCore.Mvc;
using RngHelpdesk.Api.Controllers;
using RngHelpdesk.Api.DTOs;
using RngHelpdesk.Contracts.Points.Queries;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Domain.Users;
using RngHelpdesk.Domain.Users.Events;

namespace RngHelpdesk.Api.Tests.Controllers;

public class UsersControllerTests
{
    private readonly ApiTestFixture _fixture = new();

    private UsersController CreateController()
    {
        var controller = new UsersController(
            _fixture.CreateGetAllUsersHandler(),
            _fixture.CreateGetUserByIdHandler(),
            _fixture.CreateGetUserByRunescapeUsernameHandler(),
            _fixture.CreateGetUsersHandler(),
            _fixture.CreateGetUserLifecycleHistoryHandler(),
            _fixture.CreateAddPointsToUserHandler(),
            _fixture.CreateRemovePointsFromUserHandler(),
            _fixture.CreateGetPointHistoryForUserHandler());

        ControllerTestHelpers.SetActingUser(controller, TestUsers.DefaultActingUserId);
        return controller;
    }

    [Fact]
    public async Task GetAllUsers_UsersExist_ReturnsOkWithAllUsers()
    {
        _fixture.CreateAndDispatchUser(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        var controller = CreateController();

        var result = await controller.GetAllUsers();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<GetAllUsersResponse>(ok.Value);
        Assert.Equal(1, response.TotalCount);
    }

    [Fact]
    public async Task GetAllUsers_NoUsers_ReturnsOkWithEmptyResponse()
    {
        var controller = CreateController();

        var result = await controller.GetAllUsers();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<GetAllUsersResponse>(ok.Value);
        Assert.Equal(0, response.TotalCount);
        Assert.Empty(response.Users);
    }

    [Fact]
    public async Task GetUserById_UserExists_ReturnsOkWithUser()
    {
        var user = _fixture.CreateAndDispatchUser(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        var controller = CreateController();

        var result = await controller.GetUserById(user.Id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<GetUserResponse>(ok.Value);
        Assert.Equal(user.Id, response.Id);
    }

    [Fact]
    public async Task GetUserById_UserDoesNotExist_ReturnsNotFoundWithHandlerError()
    {
        var controller = CreateController();

        var result = await controller.GetUserById(999);

        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("User not found.", notFound.Value);
    }

    [Fact]
    public async Task GetUserByRsn_UsernameLinkedToUser_ReturnsOkWithUser()
    {
        var user = _fixture.CreateAndDispatchUser(
            TestUsers.DefaultActingUserId,
            TestUsers.ValidDiscordAccount(),
            [new RunescapeAccount("Zezima")]);
        var controller = CreateController();

        var result = await controller.GetUserByRsn("Zezima");

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<GetUserResponse>(ok.Value);
        Assert.Equal(user.Id, response.Id);
    }

    [Fact]
    public async Task GetUserByRsn_NoMatchingUser_ReturnsNotFoundWithHandlerError()
    {
        var controller = CreateController();

        var result = await controller.GetUserByRsn("Zezima");

        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("User not found.", notFound.Value);
    }

    [Fact]
    public async Task GetUsersByPreviousRsn_HistoricalUsernameMatches_ReturnsOkWithUsers()
    {
        var user = _fixture.CreateAndDispatchUser(
            TestUsers.DefaultActingUserId,
            TestUsers.ValidDiscordAccount(),
            [new RunescapeAccount("Zezima")]);
        user.RemoveRunescapeAccount(TestUsers.DefaultActingUserId, "Zezima");
        _fixture.EventDispatcher.Dispatch(_fixture.UserRepository.Save(user));
        var controller = CreateController();

        var result = await controller.GetUsersByPreviousRsn("Zezima");

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<GetUsersResponse>(ok.Value);
        var returned = Assert.Single(response.Users);
        Assert.Equal(user.Id, returned.Id);
    }

    [Fact]
    public async Task GetUsersByPreviousRsn_NoMatchingUsers_ReturnsNotFoundWithHandlerError()
    {
        var controller = CreateController();

        var result = await controller.GetUsersByPreviousRsn("Zezima");

        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("User not found.", notFound.Value);
    }

    [Fact]
    public async Task GetUserLifecycle_UserHasHistory_ReturnsOkWithHistory()
    {
        var user = _fixture.CreateAndDispatchUser(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        _fixture.UserLifecycleHistoryProjection.Project(UserDeactivatedEvent.Create(TestUsers.DefaultActingUserId, user.Id));
        var controller = CreateController();

        var result = await controller.GetUserLifecycle(user.Id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<GetUserLifecycleHistoryResponse>(ok.Value);
        var item = Assert.Single(response.History);
        Assert.Equal("Deactivated", item.Action);
    }

    [Fact]
    public async Task GetUserLifecycle_UserDoesNotExist_ReturnsNotFoundWithHandlerError()
    {
        var controller = CreateController();

        var result = await controller.GetUserLifecycle(999);

        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("User not found.", notFound.Value);
    }

    [Fact]
    public async Task AddPoints_ValidRequest_ReturnsNoContentAndUpdatesTotal()
    {
        var user = _fixture.CreateAndDispatchUser(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        var controller = CreateController();

        var result = await controller.AddPoints(user.Id, new AddPointsDto { Points = 50, Reason = "Boss kill" });

        Assert.IsType<NoContentResult>(result);
        Assert.True(_fixture.UserSummaryProjection.TryGetById(user.Id, out var summary));
        Assert.Equal(50, summary!.ClanPoints);
    }

    [Fact]
    public async Task AddPoints_UserDoesNotExist_ReturnsBadRequestWithHandlerError()
    {
        var controller = CreateController();

        var result = await controller.AddPoints(999, new AddPointsDto { Points = 50, Reason = "Boss kill" });

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("User not found.", badRequest.Value);
    }

    [Fact]
    public async Task RemovePoints_ValidRequest_ReturnsNoContentAndUpdatesTotal()
    {
        var user = _fixture.CreateAndDispatchUser(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        user.AddClanPoints(TestUsers.DefaultActingUserId, 50, "Boss kill");
        _fixture.EventDispatcher.Dispatch(_fixture.UserRepository.Save(user));
        var controller = CreateController();

        var result = await controller.RemovePoints(user.Id, new RemovePointsDto { Points = 20, Reason = "Rule violation" });

        Assert.IsType<NoContentResult>(result);
        Assert.True(_fixture.UserSummaryProjection.TryGetById(user.Id, out var summary));
        Assert.Equal(30, summary!.ClanPoints);
    }

    [Fact]
    public async Task RemovePoints_DeductionWouldGoBelowZero_ReturnsBadRequestWithHandlerError()
    {
        var user = _fixture.CreateAndDispatchUser(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        var controller = CreateController();

        var result = await controller.RemovePoints(user.Id, new RemovePointsDto { Points = 10, Reason = "Rule violation" });

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Cannot deduct clan points below zero.", badRequest.Value);
    }

    [Fact]
    public async Task GetPointHistoryForUser_ValidRequest_ReturnsOkWithHistory()
    {
        var user = _fixture.CreateAndDispatchUser(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        user.AddClanPoints(TestUsers.DefaultActingUserId, 50, "Boss kill");
        _fixture.EventDispatcher.Dispatch(_fixture.UserRepository.Save(user));
        var controller = CreateController();

        var result = await controller.GetPointHistoryForUser(user.Id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<GetPointHistoryForUserResponse>(ok.Value);
        Assert.Equal(2, response.TotalEventCount);
    }

    [Fact]
    public async Task GetPointHistoryForUser_UserDoesNotExist_ReturnsNotFoundWithHandlerError()
    {
        var controller = CreateController();

        var result = await controller.GetPointHistoryForUser(999);

        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("User not found", notFound.Value);
    }
}

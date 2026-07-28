using Microsoft.AspNetCore.Mvc;
using RngHelpdesk.Api.Controllers;
using RngHelpdesk.Api.DTOs;
using RngHelpdesk.Contracts.Models.Users.Dtos;
using RngHelpdesk.Contracts.Users.Queries;
using RngHelpdesk.Contracts.Users.Views;
using RngHelpdesk.Domain.Users;

namespace RngHelpdesk.Api.Tests.Controllers;

public class RunescapeAccountsControllerTests
{
    private readonly ApiTestFixture _fixture = new();

    private RunescapeAccountsController CreateController()
    {
        var controller = new RunescapeAccountsController(
            _fixture.CreateGetRunescapeAccountHandler(),
            _fixture.CreateGetPreviousRunescapeAccountsHandler(),
            _fixture.CreateGetRunescapeAccountHistoryHandler(),
            _fixture.CreateLinkRunescapeAccountHandler(),
            _fixture.CreateDelinkRunescapeAccountHandler(),
            _fixture.CreateRenameRunescapeAccountHandler());

        ControllerTestHelpers.SetActingUser(controller, TestUsers.DefaultActingUserId);
        return controller;
    }

    [Fact]
    public async Task GetRunescapeAccounts_UserExists_ReturnsOkWithLinkedAccounts()
    {
        var user = await _fixture.CreateAndDispatchUserAsync(
            TestUsers.DefaultActingUserId,
            TestUsers.ValidDiscordAccount(),
            [new RunescapeAccount("Zezima")]);
        var controller = CreateController();

        var result = await controller.GetRunescapeAccounts(user.Id, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<GetRunescapeAccountsResponse>(ok.Value);
        var account = Assert.Single(response.Accounts);
        Assert.Equal("Zezima", account.Username);
    }

    [Fact]
    public async Task GetRunescapeAccounts_UserDoesNotExist_ReturnsBadRequestWithHandlerError()
    {
        var controller = CreateController();

        var result = await controller.GetRunescapeAccounts(999, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("User not found.", badRequest.Value);
    }

    [Fact]
    public async Task GetPreviousRunescapeAccountUsernames_UserHasDelinkHistory_ReturnsOkWithPreviousAccounts()
    {
        var user = await _fixture.CreateAndDispatchUserAsync(
            TestUsers.DefaultActingUserId,
            TestUsers.ValidDiscordAccount(),
            [new RunescapeAccount("Zezima")]);
        user.RemoveRunescapeAccount(TestUsers.DefaultActingUserId, "Zezima");
        _fixture.EventDispatcher.Dispatch(await _fixture.UserRepository.SaveAsync(user));
        var controller = CreateController();

        var result = await controller.GetPreviousRunescapeAccountUsernames(user.Id, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<GetRunescapeAccountsResponse>(ok.Value);
        var account = Assert.Single(response.Accounts);
        Assert.Equal("Zezima", account.Username);
    }

    [Fact]
    public async Task GetPreviousRunescapeAccountUsernames_NoHistory_ReturnsOkWithEmptyAccounts()
    {
        var controller = CreateController();

        var result = await controller.GetPreviousRunescapeAccountUsernames(999, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<GetRunescapeAccountsResponse>(ok.Value);
        Assert.Empty(response.Accounts);
    }

    [Fact]
    public async Task LinkRunescapeAccount_ValidRequest_ReturnsOkAndLinksAccount()
    {
        var user = await _fixture.CreateAndDispatchUserAsync(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        var controller = CreateController();

        var result = await controller.LinkRunescapeAccount(user.Id, new RunescapeAccountDto { Username = "Zezima" }, CancellationToken.None);

        Assert.IsType<OkResult>(result);
        var summary = await _fixture.UserSummaryProjection.GetByIdAsync(user.Id);
        Assert.NotNull(summary);
        Assert.Contains(summary!.RunescapeAccounts, a => a.Username == "Zezima");
    }

    [Fact]
    public async Task LinkRunescapeAccount_InvalidUsername_ReturnsBadRequestWithValidationError()
    {
        var user = await _fixture.CreateAndDispatchUserAsync(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        var controller = CreateController();

        var result = await controller.LinkRunescapeAccount(user.Id, new RunescapeAccountDto { Username = "Zez!ma" }, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid RuneScape username.", badRequest.Value);
    }

    [Fact]
    public async Task LinkRunescapeAccount_UserDoesNotExist_ReturnsBadRequestWithHandlerError()
    {
        var controller = CreateController();

        var result = await controller.LinkRunescapeAccount(999, new RunescapeAccountDto { Username = "Zezima" }, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("User not found.", badRequest.Value);
    }

    [Fact]
    public async Task DelinkRunescapeAccount_ValidRequest_ReturnsOkAndRemovesAccount()
    {
        var user = await _fixture.CreateAndDispatchUserAsync(
            TestUsers.DefaultActingUserId,
            TestUsers.ValidDiscordAccount(),
            [new RunescapeAccount("Zezima")]);
        var controller = CreateController();

        var result = await controller.DelinkRunescapeAccount(user.Id, new RunescapeAccountDto { Username = "Zezima" }, CancellationToken.None);

        Assert.IsType<OkResult>(result);
        var summary = await _fixture.UserSummaryProjection.GetByIdAsync(user.Id);
        Assert.NotNull(summary);
        Assert.Empty(summary!.RunescapeAccounts);
    }

    [Fact]
    public async Task DelinkRunescapeAccount_UsernameNotLinked_ReturnsBadRequestWithHandlerError()
    {
        var user = await _fixture.CreateAndDispatchUserAsync(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        var controller = CreateController();

        var result = await controller.DelinkRunescapeAccount(user.Id, new RunescapeAccountDto { Username = "Zezima" }, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Runescape account not linked.", badRequest.Value);
    }

    [Fact]
    public async Task RenameRunescapeAccount_ValidRequest_ReturnsNoContentAndRenamesAccount()
    {
        var user = await _fixture.CreateAndDispatchUserAsync(
            TestUsers.DefaultActingUserId,
            TestUsers.ValidDiscordAccount(),
            [new RunescapeAccount("Zezima")]);
        var controller = CreateController();

        var result = await controller.RenameRunescapeAccount(user.Id, new RenameRunescapeAccountDto("Zezima", "NotZezima"), CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        var summary = await _fixture.UserSummaryProjection.GetByIdAsync(user.Id);
        Assert.NotNull(summary);
        Assert.Contains(summary!.RunescapeAccounts, a => a.Username == "NotZezima");
    }

    [Fact]
    public async Task RenameRunescapeAccount_OldUsernameNotLinked_ReturnsBadRequestWithHandlerError()
    {
        var user = await _fixture.CreateAndDispatchUserAsync(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        var controller = CreateController();

        var result = await controller.RenameRunescapeAccount(user.Id, new RenameRunescapeAccountDto("Zezima", "NotZezima"), CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Runescape account not found.", badRequest.Value);
    }

    [Fact]
    public async Task GetRunescapeAccountHistory_UserHasHistory_ReturnsOkWithFullHistory()
    {
        var user = await _fixture.CreateAndDispatchUserAsync(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        user.AddRunescapeAccount(TestUsers.DefaultActingUserId, "Zezima");
        _fixture.EventDispatcher.Dispatch(await _fixture.UserRepository.SaveAsync(user));
        var controller = CreateController();

        var result = await controller.GetRunescapeAccountHistory(user.Id, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<GetRunescapeAccountHistoryResponse>(ok.Value);
        var item = Assert.Single(response.History);
        Assert.Equal(RunescapeAccountChangeType.Linked, item.ChangeType);
    }

    [Fact]
    public async Task GetRunescapeAccountHistory_NoHistory_ReturnsOkWithEmptyHistory()
    {
        var controller = CreateController();

        var result = await controller.GetRunescapeAccountHistory(999, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<GetRunescapeAccountHistoryResponse>(ok.Value);
        Assert.Empty(response.History);
    }
}

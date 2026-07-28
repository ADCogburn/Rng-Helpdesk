using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Models.Users.Dtos;
using RngHelpdesk.Contracts.Users.Commands;

namespace RngHelpdesk.Operations.Tests.Admin;

public class CreateUserHandlerTests
{
    private readonly OperationsTestFixture _fixture = new();

    private CreateUserHandler CreateHandler()
        => new(_fixture.UserSummaryProjection, _fixture.UserRepository, _fixture.EventDispatcher, _fixture.CredentialStore);

    [Fact]
    public async Task Handle_DiscordAccountIsNull_ReturnsFailure()
    {
        var handler = CreateHandler();
        var request = new CreateUserRequest { ActingUserId = TestUsers.DefaultActingUserId, DiscordAccount = null! };

        var result = await handler.Handle(request);

        Assert.Equal(ResultStatus.Failure, result.Status);
        Assert.Equal("Discord account is required.", result.Error);
    }

    [Theory]
    [InlineData(0, "validUsername")]
    [InlineData(TestUsers.DefaultDiscordId, "")]
    [InlineData(TestUsers.DefaultDiscordId, " ")]
    public async Task Handle_DiscordAccountIsMalformed_ReturnsFailure(ulong discordId, string username)
    {
        var handler = CreateHandler();
        var request = new CreateUserRequest
        {
            ActingUserId = TestUsers.DefaultActingUserId,
            DiscordAccount = new DiscordAccountDto { DiscordId = discordId, Username = username }
        };

        var result = await handler.Handle(request);

        Assert.Equal(ResultStatus.Failure, result.Status);
        Assert.Equal("Discord account, its snowflake Id, and its username are required.", result.Error);
    }

    [Fact]
    public async Task Handle_DiscordIdAlreadyExists_ReturnsFailure()
    {
        _fixture.CreateAndDispatchUser(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        var handler = CreateHandler();
        var request = new CreateUserRequest
        {
            ActingUserId = TestUsers.DefaultActingUserId,
            DiscordAccount = new DiscordAccountDto { DiscordId = TestUsers.DefaultDiscordId, Username = "someOtherName" }
        };

        var result = await handler.Handle(request);

        Assert.Equal(ResultStatus.Failure, result.Status);
        Assert.Equal("User with this Discord ID or username already exists.", result.Error);
    }

    [Fact]
    public async Task Handle_DiscordUsernameAlreadyExists_ReturnsFailure()
    {
        _fixture.CreateAndDispatchUser(TestUsers.DefaultActingUserId, TestUsers.ValidDiscordAccount());
        var handler = CreateHandler();
        var request = new CreateUserRequest
        {
            ActingUserId = TestUsers.DefaultActingUserId,
            DiscordAccount = new DiscordAccountDto { DiscordId = 999, Username = TestUsers.DefaultDiscordUsername }
        };

        var result = await handler.Handle(request);

        Assert.Equal(ResultStatus.Failure, result.Status);
        Assert.Equal("User with this Discord ID or username already exists.", result.Error);
    }

    [Fact]
    public async Task Handle_ValidRequestWithNoRunescapeAccounts_CreatesUserUsingDiscordUsernameAsPreferredUsername()
    {
        var handler = CreateHandler();
        var request = new CreateUserRequest
        {
            ActingUserId = TestUsers.DefaultActingUserId,
            DiscordAccount = new DiscordAccountDto { DiscordId = TestUsers.DefaultDiscordId, Username = TestUsers.DefaultDiscordUsername }
        };

        var result = await handler.Handle(request);

        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.Equal(TestUsers.DefaultDiscordId, result.Value!.UserId);
        Assert.Equal(TestUsers.DefaultDiscordUsername.ToLowerInvariant(), result.Value.Username);
        Assert.NotEmpty(result.Value.TemporaryPassword);
        Assert.True(_fixture.UserSummaryProjection.TryGetById(TestUsers.DefaultDiscordId, out _));
    }

    [Fact]
    public async Task Handle_ValidRequestWithRunescapeAccounts_PrefersFirstRunescapeUsernameForCredentials()
    {
        var handler = CreateHandler();
        var request = new CreateUserRequest
        {
            ActingUserId = TestUsers.DefaultActingUserId,
            DiscordAccount = new DiscordAccountDto { DiscordId = TestUsers.DefaultDiscordId, Username = TestUsers.DefaultDiscordUsername },
            RunescapeAccounts = [new RunescapeAccountDto { Username = "Zezima" }]
        };

        var result = await handler.Handle(request);

        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.Equal("zezima", result.Value!.Username);
    }
}

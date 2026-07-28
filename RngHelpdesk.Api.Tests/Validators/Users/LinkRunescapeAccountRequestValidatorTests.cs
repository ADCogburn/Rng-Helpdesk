using RngHelpdesk.Api.Validators.Users;

namespace RngHelpdesk.Api.Tests.Validators.Users;

public class LinkRunescapeAccountRequestValidatorTests
{
    private const string ExpectedErrorMessage = "Invalid RuneScape username.";

    private readonly LinkRunescapeAccountRequestValidator _validator = new();

    [Fact]
    public void Validate_ValidUsername_IsValid()
    {
        var request = new LinkRunescapeAccountRequest(ActingUserId: 1, UserId: 2, Username: "Zezima");

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ThirteenChars")]
    [InlineData(" Zezima")]
    [InlineData("Zezima ")]
    [InlineData("Zez!ma")]
    public void Validate_InvalidUsername_ReturnsSingleErrorOnUsername(string username)
    {
        var request = new LinkRunescapeAccountRequest(ActingUserId: 1, UserId: 2, Username: username);

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors);
        Assert.Equal(nameof(LinkRunescapeAccountRequest.Username), error.PropertyName);
        Assert.Equal(ExpectedErrorMessage, error.ErrorMessage);
    }
}

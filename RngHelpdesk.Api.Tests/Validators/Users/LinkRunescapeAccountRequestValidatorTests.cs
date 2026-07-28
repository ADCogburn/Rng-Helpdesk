using RngHelpdesk.Api.Validators.Users;

namespace RngHelpdesk.Api.Tests.Validators.Users;

public class LinkRunescapeAccountRequestValidatorTests
{
    private readonly LinkRunescapeAccountRequestValidator _validator = new();

    [Fact]
    public void Validate_ValidUsername_IsValid()
    {
        var request = new LinkRunescapeAccountRequest(ActingUserId: 1, UserId: 2, Username: "Zezima");

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyUsername_IsInvalid()
    {
        var request = new LinkRunescapeAccountRequest(ActingUserId: 1, UserId: 2, Username: "");

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LinkRunescapeAccountRequest.Username)
            && e.ErrorMessage == "Invalid RuneScape username.");
    }

    [Fact]
    public void Validate_WhitespaceUsername_IsInvalid()
    {
        var request = new LinkRunescapeAccountRequest(ActingUserId: 1, UserId: 2, Username: "   ");

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LinkRunescapeAccountRequest.Username)
            && e.ErrorMessage == "Invalid RuneScape username.");
    }

    [Fact]
    public void Validate_UsernameOverTwelveCharacters_IsInvalid()
    {
        var request = new LinkRunescapeAccountRequest(ActingUserId: 1, UserId: 2, Username: "ThirteenChars");

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LinkRunescapeAccountRequest.Username)
            && e.ErrorMessage == "Invalid RuneScape username.");
    }

    [Fact]
    public void Validate_UsernameWithLeadingSpace_IsInvalid()
    {
        var request = new LinkRunescapeAccountRequest(ActingUserId: 1, UserId: 2, Username: " Zezima");

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LinkRunescapeAccountRequest.Username)
            && e.ErrorMessage == "Invalid RuneScape username.");
    }

    [Fact]
    public void Validate_UsernameWithTrailingSpace_IsInvalid()
    {
        var request = new LinkRunescapeAccountRequest(ActingUserId: 1, UserId: 2, Username: "Zezima ");

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LinkRunescapeAccountRequest.Username)
            && e.ErrorMessage == "Invalid RuneScape username.");
    }

    [Fact]
    public void Validate_UsernameWithDisallowedCharacter_IsInvalid()
    {
        var request = new LinkRunescapeAccountRequest(ActingUserId: 1, UserId: 2, Username: "Zez!ma");

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LinkRunescapeAccountRequest.Username)
            && e.ErrorMessage == "Invalid RuneScape username.");
    }
}

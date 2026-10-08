using RngHelpdesk.Api.DTOs;
using RngHelpdesk.Api.Validators.Auth;

namespace RngHelpdesk.Api.Tests.Validators.Auth;

public class ChangePasswordDtoValidatorTests
{
    private readonly ChangePasswordDtoValidator _validator = new();

    [Fact]
    public void Validate_LongEnoughAndDifferent_IsValid()
    {
        var result = _validator.Validate(new ChangePasswordDto { CurrentPassword = "old-password", NewPassword = "new-password" });

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("short")]
    [InlineData("1234567")]
    public void Validate_TooShort_ReturnsErrorOnNewPassword(string newPassword)
    {
        var result = _validator.Validate(new ChangePasswordDto { CurrentPassword = "old-password", NewPassword = newPassword });

        var error = Assert.Single(result.Errors);
        Assert.Equal(nameof(ChangePasswordDto.NewPassword), error.PropertyName);
    }

    [Fact]
    public void Validate_SameAsCurrent_ReturnsErrorOnNewPassword()
    {
        var result = _validator.Validate(new ChangePasswordDto { CurrentPassword = "same-password", NewPassword = "same-password" });

        var error = Assert.Single(result.Errors);
        Assert.Equal(nameof(ChangePasswordDto.NewPassword), error.PropertyName);
    }
}
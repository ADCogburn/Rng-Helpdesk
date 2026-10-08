using FluentValidation;
using RngHelpdesk.Api.DTOs;

namespace RngHelpdesk.Api.Validators.Auth;

public sealed class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
{
    public ChangePasswordDtoValidator()
    {
        RuleFor(x => x.NewPassword)
            .Cascade(CascadeMode.Stop)
            .MinimumLength(8).WithMessage("New password must be at least 8 characters.")
            .NotEqual(x => x.CurrentPassword).WithMessage("New password must differ from the current password.");
    }
}

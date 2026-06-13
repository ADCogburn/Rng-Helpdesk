using FluentValidation;

namespace RngHelpdesk.Api.Validators.Users;

public sealed class LinkRunescapeAccountRequestValidator : AbstractValidator<LinkRunescapeAccountRequest>
{
    public LinkRunescapeAccountRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .Matches("^(?! )[A-Za-z0-9 -]{1,12}(?<! )$") // RuneScape username rules - alphanumeric & no space at end or beginning
            .WithMessage("Invalid RuneScape username.");
    }
}
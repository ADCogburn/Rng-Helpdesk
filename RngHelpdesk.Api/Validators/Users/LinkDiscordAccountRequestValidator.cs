using FluentValidation;

namespace RngHelpdesk.Api.Validators.Users;

public sealed class LinkDiscordAccountRequestValidator : AbstractValidator<LinkDiscordAccountRequest>
{
    public LinkDiscordAccountRequestValidator()
    {
        RuleFor(x => x.DiscordId)
            .GreaterThan(0UL)
            .WithMessage("DiscordId must be a valid snowflake.");
    }
}
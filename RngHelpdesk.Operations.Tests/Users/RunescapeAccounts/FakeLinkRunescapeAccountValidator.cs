using FluentValidation;
using FluentValidation.Results;

namespace RngHelpdesk.Operations.Tests.Users.RunescapeAccounts;

/// <summary>
/// Hand-rolled IValidator&lt;LinkRunescapeAccountRequest&gt; test double. LinkRunescapeAccountHandler
/// only ever calls the generic Validate(T) overload; the real validator lives in RngHelpdesk.Api,
/// which Operations.Tests cannot reference, so its rule content is out of scope here (see #31).
/// </summary>
internal sealed class FakeLinkRunescapeAccountValidator : IValidator<LinkRunescapeAccountRequest>
{
    private readonly ValidationResult _result;

    public FakeLinkRunescapeAccountValidator(ValidationResult result)
    {
        _result = result;
    }

    public static FakeLinkRunescapeAccountValidator Passing() => new(new ValidationResult());

    public static FakeLinkRunescapeAccountValidator Failing(params string[] errorMessages) => new(
        new ValidationResult(errorMessages.Select(m => new ValidationFailure("Username", m))));

    public ValidationResult Validate(LinkRunescapeAccountRequest instance) => _result;

    public Task<ValidationResult> ValidateAsync(LinkRunescapeAccountRequest instance, CancellationToken cancellation = default)
        => Task.FromResult(_result);

    public ValidationResult Validate(IValidationContext context) => throw new NotSupportedException();

    public Task<ValidationResult> ValidateAsync(IValidationContext context, CancellationToken cancellation = default)
        => throw new NotSupportedException();

    public IValidatorDescriptor CreateDescriptor() => throw new NotSupportedException();

    public bool CanValidateInstancesOfType(Type type) => throw new NotSupportedException();
}

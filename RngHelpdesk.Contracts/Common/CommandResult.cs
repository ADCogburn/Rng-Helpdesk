namespace RngHelpdesk.Contracts.Common;

public class CommandResult
{
    public bool Success => Status == ResultStatus.Success;

    public ResultStatus Status { get; }
    public string? Error { get; }

    protected CommandResult(ResultStatus status, string? error)
    {
        Status = status;
        Error = error;
    }

    public static CommandResult Ok()
        => new(ResultStatus.Success, null);

    public static CommandResult Fail(string error)
        => new(ResultStatus.Failure, error);

    public static CommandResult NotFound(string? error)
        => new(ResultStatus.NotFound, error);
}
public sealed class CommandResult<T> : CommandResult
{
    public T? Value { get; }

    private CommandResult(ResultStatus status, string? error, T? value)
        : base(status, error)
    {
        Value = value;
    }

    public static CommandResult<T> Ok(T value)
        => new(ResultStatus.Success, null, value);

    public static new CommandResult<T> Fail(string error)
        => new(ResultStatus.Failure, error, default);

    public static new CommandResult<T> NotFound(string error)
        => new(ResultStatus.NotFound, error, default);
}

public enum ResultStatus
{
    Success,
    Failure,
    NotFound
}
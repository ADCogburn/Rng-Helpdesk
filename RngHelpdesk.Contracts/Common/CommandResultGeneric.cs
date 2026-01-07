namespace RngHelpdesk.Contracts.Common;

public sealed class CommandResult<T> : CommandResult
{
    public T? Value { get; init; }

    public static CommandResult<T> Ok(T value) => new() { Success = true, Value = value };
    public static new CommandResult<T> Fail(string error) => new() { Success = false, Error = error };
}

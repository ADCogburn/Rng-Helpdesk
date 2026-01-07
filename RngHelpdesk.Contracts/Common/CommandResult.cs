namespace RngHelpdesk.Contracts.Common;

public class CommandResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }

    public static CommandResult Ok() => new() { Success = true };

    public static CommandResult Fail(string error) => new() { Success = false, Error = error };
}

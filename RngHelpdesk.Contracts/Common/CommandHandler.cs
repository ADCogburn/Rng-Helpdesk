namespace RngHelpdesk.Contracts.Common;

/// <summary>
/// Command Handler wraps the try/catch pattern around handler logic that calls Domain or Infra work.
/// This is to keep the code there concise and clear, but also catch the exceptions and handle them with a proper CommandResult.Fail().
/// </summary>
public static class CommandHandler
{
    public static async Task<CommandResult> ExecuteAsync(Func<Task> action)
    {
        try
        {
            await action();
            return CommandResult.Ok();

        }
        catch (AggregateException ex)
        {
            return CommandResult.NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return CommandResult.Fail(ex.Message);
        }
    }

    public static async Task<CommandResult<T>> ExecuteAsync<T>(Func<Task<T>> action)
    {
        try
        {
            var value = await action();
            return CommandResult<T>.Ok(value);
        }
        catch (Exception ex)
        {
            return CommandResult<T>.Fail(ex.Message);
        }
    }
}

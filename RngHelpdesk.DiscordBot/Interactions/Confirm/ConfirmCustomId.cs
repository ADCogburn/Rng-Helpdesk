namespace RngHelpdesk.DiscordBot.Interactions.Confirm;

public enum ConfirmAction { Deactivate, Promote, Demote }

/// <summary>Custom ids for the confirm/cancel buttons: <c>confirm:{action}:{targetId}:{invokerId}</c>.</summary>
public static class ConfirmCustomId
{
    public const string ConfirmPattern = "confirm:*:*:*";
    public const string CancelPattern = "cancel:*";

    public static string Confirm(ConfirmAction action, ulong targetId, ulong invokerId)
        => $"confirm:{action.ToString().ToLowerInvariant()}:{targetId}:{invokerId}";

    public static string Cancel(ulong invokerId) => $"cancel:{invokerId}";

    public static bool TryParse(string action, string target, string invoker, out ConfirmAction parsedAction, out ulong targetId, out ulong invokerId)
    {
        targetId = 0;
        invokerId = 0;
        return Enum.TryParse(action, ignoreCase: true, out parsedAction)
            && Enum.IsDefined(parsedAction)
            && ulong.TryParse(target, out targetId)
            && ulong.TryParse(invoker, out invokerId);
    }
}

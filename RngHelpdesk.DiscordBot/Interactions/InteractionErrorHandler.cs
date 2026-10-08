using Discord;
using Discord.Interactions;
using IResult = Discord.Interactions.IResult;

namespace RngHelpdesk.DiscordBot.Interactions;

/// <summary>Turns a failed interaction result into a single ephemeral reply, using <see cref="ErrorReplies"/>.</summary>
public static class InteractionErrorHandler
{
    public static async Task HandleAsync(IInteractionContext context, IResult result, ILogger logger)
    {
        if (result.IsSuccess)
            return;

        string message;

        if (result is ExecuteResult { Exception: { } wrapper })
        {
            var exception = wrapper is InteractionException { InnerException: { } inner } ? inner : wrapper;
            var reply = ErrorReplies.Describe(exception);
            message = reply.Message;

            if (reply.Unexpected)
                logger.LogError(exception, "Interaction failed for user {UserId}.", context.User.Id);
            else
                logger.LogInformation("Interaction rejected for user {UserId}: {Message}", context.User.Id, reply.Message);
        }
        else
        {
            logger.LogWarning("Interaction not executed ({Error}): {Reason}", result.Error, result.ErrorReason);
            message = ErrorReplies.Unavailable;
        }

        try
        {
            if (context.Interaction.HasResponded)
                await context.Interaction.FollowupAsync(message, ephemeral: true);
            else
                await context.Interaction.RespondAsync(message, ephemeral: true);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not deliver error reply to user {UserId}.", context.User.Id);
        }
    }
}

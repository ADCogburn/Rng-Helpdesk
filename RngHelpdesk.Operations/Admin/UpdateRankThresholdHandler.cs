using RngHelpdesk.Contracts.Common;
using RngHelpdesk.Contracts.Common.Ranks;
using RngHelpdesk.Contracts.Common.Ranks.Commands;
using RngHelpdesk.Operations.Common;

namespace RngHelpdesk.Operations.Admin;

public sealed class UpdateRankThresholdHandler(
    IRankThresholdProvider rankThresholdProvider,
    IRankThresholdRepository rankThresholdRepository) : ICommandHandler<UpdateRankThresholdCommand>
{
    public async Task<CommandResult> Handle(UpdateRankThresholdCommand command, CancellationToken cancellationToken = default)
    {
        // GetThresholdsAsync returns thresholds in ascending point order (the Postgres provider
        // orders by SortOrder, which matches ascending PointsRequired; the in-memory test double
        // is hardcoded in the same order) -- the monotonicity check below relies on that order.
        var thresholds = await rankThresholdProvider.GetThresholdsAsync(cancellationToken);
        var ordered = thresholds.ToList();

        var index = ordered.FindIndex(t => t.Rank == command.Rank);

        if (index < 0)
            return CommandResult.Fail($"'{command.Rank}' has no configurable point threshold.");

        if (index > 0 && command.PointsRequired <= ordered[index - 1].PointsRequired)
        {
            return CommandResult.Fail(
                $"PointsRequired must be greater than {ordered[index - 1].Rank}'s threshold ({ordered[index - 1].PointsRequired}).");
        }

        if (index < ordered.Count - 1 && command.PointsRequired >= ordered[index + 1].PointsRequired)
        {
            return CommandResult.Fail(
                $"PointsRequired must be less than {ordered[index + 1].Rank}'s threshold ({ordered[index + 1].PointsRequired}).");
        }

        return await CommandHandler.ExecuteAsync(async () =>
        {
            await rankThresholdRepository.UpdatePointsRequiredAsync(command.Rank, command.PointsRequired, cancellationToken);
        });
    }
}

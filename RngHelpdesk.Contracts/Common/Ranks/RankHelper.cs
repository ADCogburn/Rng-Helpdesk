using RngHelpdesk.Contracts.Security;

namespace RngHelpdesk.Contracts.Common.Ranks;

public static class RankHelper
{
    public static Rank FromAppRole(AppRole role) =>
        role switch
        {
            AppRole.Administrator => Rank.Administrator,
            AppRole.SuperAdministrator => Rank.DeputyOwner,
            AppRole.Owner => Rank.Owner,
            _ => throw new InvalidOperationException()
        };
}

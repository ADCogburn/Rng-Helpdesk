using RngHelpdesk.Domain.Users;

namespace RngHelpdesk.Operations.Ranks;

public static class RankHelper
{
    public static Rank FromAuthority(AuthorityRole role) =>
        role switch
        {
            AuthorityRole.Administrator => Rank.Administrator,
            AuthorityRole.SuperAdministrator => Rank.DeputyOwner,
            AuthorityRole.Owner => Rank.Owner,
            _ => throw new InvalidOperationException()
        };
}

using RngHelpdesk.Domain.Ranks;
using RngHelpdesk.Domain.Users;

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

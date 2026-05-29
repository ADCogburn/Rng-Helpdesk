namespace RngHelpdesk.Infrastructure.Persistence.Contexts;

internal sealed class ActorUserLinkRow
{
    public Guid ActorId { get; set; }
    public string ActorType { get; set; } = default!;
    public int UserId { get; set; }
}
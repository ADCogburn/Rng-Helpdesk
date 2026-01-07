public sealed class LinkDiscordAccountRequest
{
    public int UserId { get; init; }
    public string Username { get; init; } = string.Empty; // The Adapter should talk to Discord API to resolve DiscordId -> Username (and confirm correctness where needed)
    public ulong DiscordId { get; init; }
}
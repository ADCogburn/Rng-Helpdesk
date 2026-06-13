public sealed record LinkRunescapeAccountRequest(
    ulong ActingUserId,
    ulong UserId,
    string Username);
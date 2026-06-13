using System.Text.Json.Serialization;

namespace RngHelpdesk.Domain.Users;

public sealed class RunescapeAccount
{
    public string Username { get; private set; }

    [JsonConstructor]
    public RunescapeAccount(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be null or empty.", nameof(username));

        Username = username;
    }
}

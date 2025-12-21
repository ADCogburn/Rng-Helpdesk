namespace RngHelpdesk.Domain.Users;

public sealed class RunescapeAccount
{
    public int Id { get; private set; }
    public string Username { get; private set; }
    public bool IsActive { get; private set; }

    public RunescapeAccount(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be null or empty.", nameof(username));

        Username = username;
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}

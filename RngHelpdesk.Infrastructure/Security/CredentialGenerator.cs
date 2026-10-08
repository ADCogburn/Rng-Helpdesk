using System.Security.Cryptography;

namespace RngHelpdesk.Infrastructure.Security;

internal static class CredentialGenerator
{
    public static string GenerateTemporaryPassword() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(12));

    public static string NormalizeUsername(string preferredUsername)
    {
        if (string.IsNullOrWhiteSpace(preferredUsername))
            return $"user{Random.Shared.Next(1000, 9999)}";

        return preferredUsername
            .Trim()
            .Replace(" ", "")
            .ToLowerInvariant();
    }
}

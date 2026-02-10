using System.Security.Cryptography;
using System.Text;

namespace Bookazone.Domain.Common;

public static class SecurityHelpers
{
    public static string ComputeFingerprint(string userAgent, string ip, string? deviceIdentifier, string? memorySlug)
    {
        var raw = $"{userAgent}|{ip}|{deviceIdentifier ?? ""}|{memorySlug ?? ""}";
        return ComputeSha256Hash(raw);
    }

    public static string ComputeSha256Hash(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}

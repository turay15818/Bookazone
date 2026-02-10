using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Bookazone.Application.Common.Shared.Helpers;

public static class ReceiptVerification
{
    private const string Secret = "Bookazone_RECEIPT_SECRET"; 

    public static string GenerateCode(
        string verificationCode,
        Guid tenantId,
        decimal total,
        DateTime issuedAt)
    {
        var raw = $"{verificationCode}|{tenantId}|{total}|{issuedAt:O}|{Secret}";
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));

        return $"SP-{Convert.ToHexString(hash)[..12]}";
    }

    public static string BuildQrPayload(string verificationCode)
    {
        return $" http://192.168.43.25:5287/receipt/verify/{verificationCode}";
    }

}

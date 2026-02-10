using Bookazone.Application.Common.Shared.Utils;

namespace Bookazone.Application.Common.Shared.Helpers;

public static class ConnectionStringProtector
{
    public static string EncryptConnectionString(string plainConnectionString, string base64Key)
    {
        var crypto = new CryptoService(base64Key);
        return CryptoService.Encrypt(plainConnectionString);
    }
    
    

    public static string DecryptConnectionString(string encryptedConnectionString, string base64Key)
    {
        var crypto = new CryptoService(base64Key);
        return CryptoService.Decrypt(encryptedConnectionString);
    }
}

using System.Security.Cryptography;
using System.Text;

namespace Bookazone.Application.Common.Shared.Utils
{
    public class CryptoService
    {
        private static byte[]? _key;

        public CryptoService(string base64Key)
        {
            _key = Convert.FromBase64String(base64Key);
            if (_key.Length != 32)
            {
                throw new ArgumentException("Key must be 256 bits (32 bytes).");
            }
        }

        [Obsolete("Obsolete")]
        public static string Encrypt(string plainText)
        {
            {
                using var aesGcm = new AesGcm(_key);

                byte[] plaintextBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] iv = new byte[AesGcm.NonceByteSizes.MaxSize]; // 12 bytes
                byte[] tag = new byte[AesGcm.TagByteSizes.MaxSize];  // 16 bytes
                byte[] cipherBytes = new byte[plaintextBytes.Length];
                RandomNumberGenerator.Fill(iv);
                aesGcm.Encrypt(iv, plaintextBytes, cipherBytes, tag);
                byte[] result = new byte[iv.Length + tag.Length + cipherBytes.Length];
                Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                Buffer.BlockCopy(tag, 0, result, iv.Length, tag.Length);
                Buffer.BlockCopy(cipherBytes, 0, result, iv.Length + tag.Length, cipherBytes.Length);
                return Convert.ToBase64String(result);
            }
        }

        [Obsolete("Obsolete")]
        public static string Decrypt(string encryptedText)
        {
            byte[] inputBytes = Convert.FromBase64String(encryptedText);
            byte[] iv = new byte[AesGcm.NonceByteSizes.MaxSize];
            byte[] tag = new byte[AesGcm.TagByteSizes.MaxSize];
            byte[] cipherBytes = new byte[inputBytes.Length - iv.Length - tag.Length];
            Buffer.BlockCopy(inputBytes, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(inputBytes, iv.Length, tag, 0, tag.Length);
            Buffer.BlockCopy(inputBytes, iv.Length + tag.Length, cipherBytes, 0, cipherBytes.Length);
            byte[] plainBytes = new byte[cipherBytes.Length];
            using var aesGcm = new AesGcm(_key);
            aesGcm.Decrypt(iv, cipherBytes, tag, plainBytes);
            return Encoding.UTF8.GetString(plainBytes);
        }
        
        public static string HashToken(string token)
        {
            using var sha = SHA512.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(bytes);
        }

        
    }
}

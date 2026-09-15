using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Parking_web.Models.DTO;

namespace Parking_web.Helpers
{
    public static class Encryption
    {
        private static byte[] GetKey()
        {
            var key = "7vM8xK2qN4wZ1pL9sR6tY3uF5hJ0cD8eV2bG7nQ4mXs=";

            if (string.IsNullOrWhiteSpace(key))
                throw new InvalidOperationException("EncryptionKey environment variable is not configured.");

            var keyBytes = Convert.FromBase64String(key);

            if (keyBytes.Length != 32)
                throw new InvalidOperationException("EncryptionKey must contain a 32-byte Base64 key.");

            return keyBytes;
        }

        public static string EncryptQR(QREncrypt encrypt)
        {
            var json = JsonSerializer.Serialize(encrypt);
            var plaintext = Encoding.UTF8.GetBytes(json);

            var key = GetKey();

            byte[] nonce = RandomNumberGenerator.GetBytes(12);
            byte[] tag = new byte[16];
            byte[] ciphertext = new byte[plaintext.Length];

            using var aes = new AesGcm(key, 16);

            aes.Encrypt(
                nonce,
                plaintext,
                ciphertext,
                tag
            );

            byte[] result = new byte[
                nonce.Length +
                tag.Length +
                ciphertext.Length
            ];

            Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
            Buffer.BlockCopy(tag, 0, result, nonce.Length, tag.Length);
            Buffer.BlockCopy( ciphertext, 0, result, nonce.Length + tag.Length, ciphertext.Length );

            return Convert.ToBase64String(result);
        }

        public static QREncrypt? DecryptQR(string encrypted)
        {
            try
            {
                var data = Convert.FromBase64String(encrypted);

                if (data.Length < 28)
                    return null;

                var key = GetKey();

                byte[] nonce = data[..12];
                byte[] tag = data[12..28];
                byte[] ciphertext = data[28..];

                byte[] plaintext = new byte[ciphertext.Length];

                using var aes = new AesGcm(key, 16);

                aes.Decrypt(
                    nonce,
                    ciphertext,
                    tag,
                    plaintext
                );

                var json = Encoding.UTF8.GetString(plaintext);

                return JsonSerializer.Deserialize<QREncrypt>(json);
            }
            catch
            {
                return null;
            }
        }
    }
}
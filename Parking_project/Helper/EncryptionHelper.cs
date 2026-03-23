using System.Security.Cryptography;
using System.Text;

namespace Parking_project.Helper
{
    public static class EncryptionHelper
    {
        // 32 bytes = AES-256 key
        private static readonly string Key = "8564fryhvchnilm04ygdecz4ryfcs342";
        private static readonly string IV = "2lijmc5f9kfbes4n";

        public static string Encrypt(string plainText)
        {
            using Aes aes = Aes.Create();

            aes.Key = Encoding.UTF8.GetBytes(Key);
            aes.IV = Encoding.UTF8.GetBytes(IV);

            ICryptoTransform encryptor = aes.CreateEncryptor();

            using MemoryStream ms = new MemoryStream();
            using CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            using StreamWriter sw = new StreamWriter(cs);

            sw.Write(plainText);
            sw.Close();

            return Convert.ToBase64String(ms.ToArray());
        }

        public static string Decrypt(string cipherText)
        {
            using Aes aes = Aes.Create();

            aes.Key = Encoding.UTF8.GetBytes(Key);
            aes.IV = Encoding.UTF8.GetBytes(IV);

            ICryptoTransform decryptor = aes.CreateDecryptor();

            using MemoryStream ms = new MemoryStream(Convert.FromBase64String(cipherText));
            using CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using StreamReader sr = new StreamReader(cs);

            return sr.ReadToEnd();
        }
    }
}
using System;
using System.Security.Cryptography;
using System.Text;

namespace X990TesterCore
{
    public static class CryptoService
    {
        public static EncryptedPacket Encrypt(string plainJson)
        {
            // 1. Generate AES-256 key
            byte[] aesKey;
            byte[] encryptedData;

            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.PKCS7;
                aes.GenerateKey();

                aesKey = aes.Key;

                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                {
                    byte[] plainBytes = Encoding.UTF8.GetBytes(plainJson);
                    encryptedData = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                }
            }

            // 2. Encrypt AES key using Terminal RSA (PKCS1)
            byte[] encryptedAesKey = AppSession.TerminalRsa.Encrypt(aesKey, false);

            // 3. Build packet
            return new EncryptedPacket
            {
                EncryptedAesKey = Convert.ToBase64String(encryptedAesKey),
                EncryptedData = Convert.ToBase64String(encryptedData)
            };
        }

        public static string Decrypt(EncryptedPacket packet)
        {
            // 1. Decrypt AES key using PC private RSA
            byte[] aesKey = AppSession.PcRsa.Decrypt(
                Convert.FromBase64String(packet.EncryptedAesKey),
                false);

            // 2. Decrypt data using AES
            using (Aes aes = Aes.Create())
            {
                aes.Key = aesKey;
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                {
                    byte[] encryptedData = Convert.FromBase64String(packet.EncryptedData);
                    byte[] plainBytes = decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);

                    return Encoding.UTF8.GetString(plainBytes);
                }
            }
        }
    }
}

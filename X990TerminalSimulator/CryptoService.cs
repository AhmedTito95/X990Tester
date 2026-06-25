using System;
using System.Security.Cryptography;
using System.Text;

namespace X990TerminalSimulator
{
    public static class CryptoService
    {
        /// <summary>
        /// Decrypts an incoming encrypted request packet from the client.
        /// </summary>
        public static string Decrypt(EncryptedPacket packet)
        {
            if (packet == null)
                throw new ArgumentNullException(nameof(packet));

            // 1. Load simulator's own RSA key pair (Terminal Private Key)
            using (RSACryptoServiceProvider terminalRsa = KeyStorageService.LoadOrCreateTerminalKey())
            {
                // 2. Decrypt the dynamic AES key using Terminal Private RSA (PKCS1 padding)
                byte[] aesKey = terminalRsa.Decrypt(
                    Convert.FromBase64String(packet.EncryptedAesKey),
                    false);

                // 3. Decrypt the JSON payload using AES-256 ECB PKCS7
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

        /// <summary>
        /// Encrypts a plain text JSON response for the client.
        /// </summary>
        public static EncryptedPacket Encrypt(string plainJson)
        {
            // 1. Generate a new dynamic 256-bit AES key
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

            // 2. Load the Client's public RSA key (PC Public Key) saved during INIT
            using (RSACryptoServiceProvider clientRsa = KeyStorageService.LoadClientPublicKey())
            {
                if (clientRsa == null)
                    throw new InvalidOperationException("Client PC Public Key not found. Run INIT exchange first.");

                // 3. Encrypt the dynamic AES key using the Client's Public RSA key (PKCS1 padding)
                byte[] encryptedAesKey = clientRsa.Encrypt(aesKey, false);

                // 4. Build the encrypted packet response
                return new EncryptedPacket
                {
                    EncryptedAesKey = Convert.ToBase64String(encryptedAesKey),
                    EncryptedData = Convert.ToBase64String(encryptedData)
                };
            }
        }
    }
}

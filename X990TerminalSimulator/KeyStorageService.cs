using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace X990TerminalSimulator
{
    public static class KeyStorageService
    {
        private const string TerminalKeyFileName = "simulator_terminal.xml";
        private const string ClientKeyFileName = "client_pc.key";

        private static string AppDirectory =>
            Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? string.Empty;

        private static string TerminalKeyPath =>
            Path.Combine(AppDirectory, TerminalKeyFileName);

        private static string ClientKeyPath =>
            Path.Combine(AppDirectory, ClientKeyFileName);

        /// <summary>
        /// Loads the Simulator's Terminal RSA key pair from a local XML file.
        /// Generates a new 4096-bit key if it does not yet exist.
        /// </summary>
        public static RSACryptoServiceProvider LoadOrCreateTerminalKey()
        {
            var rsa = new RSACryptoServiceProvider(4096);
            if (File.Exists(TerminalKeyPath))
            {
                string xml = File.ReadAllText(TerminalKeyPath, Encoding.UTF8);
                rsa.FromXmlString(xml);
            }
            else
            {
                string xml = rsa.ToXmlString(true);
                File.WriteAllText(TerminalKeyPath, xml, Encoding.UTF8);
            }

            return rsa;
        }

        /// <summary>
        /// Saves the client's PC public key (as Base64 DER-encoded SubjectPublicKeyInfo).
        /// </summary>
        public static void SaveClientPublicKey(string b64SubjectPublicKeyInfo)
        {
            if (string.IsNullOrEmpty(b64SubjectPublicKeyInfo))
                throw new ArgumentException("Client public key must not be empty.");

            File.WriteAllText(ClientKeyPath, b64SubjectPublicKeyInfo, Encoding.UTF8);
        }

        /// <summary>
        /// Loads the client's public key from file.
        /// </summary>
        public static RSACryptoServiceProvider LoadClientPublicKey()
        {
            if (!File.Exists(ClientKeyPath))
                return null;

            string b64 = File.ReadAllText(ClientKeyPath, Encoding.UTF8).Trim();
            if (string.IsNullOrEmpty(b64))
                return null;

            byte[] der = Convert.FromBase64String(b64);

            var rsa = new RSACryptoServiceProvider();
            rsa.ImportSubjectPublicKeyInfo(der, out _);
            return rsa;
        }
    }
}

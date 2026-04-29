using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace X990TesterCore
{
    /// <summary>
    /// Manages secure persistence of the PC RSA key pair and the Terminal public key.
    ///
    /// Strategy:
    ///   - PC RSA Key Pair:  Stored in the Windows CNG/CSP Key Store under a named
    ///                       container ("X990TesterKeyContainer"). This means the private
    ///                       key never leaves the OS key store in plain text.
    ///   - Terminal Public Key: Stored as a Base64 DER-encoded file next to the exe
    ///                          (terminal.key). The public key is not secret, so file
    ///                          storage is sufficient.
    /// </summary>
    public static class KeyStorageService
    {
        private const string ContainerName = "X990TesterKeyContainer";
        private const string TerminalKeyFileName = "terminal.key";

        // Base directory next to the executable
        private static string AppDirectory =>
            Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        private static string TerminalKeyPath =>
            Path.Combine(AppDirectory, TerminalKeyFileName);

        // ─── PC RSA Key ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Loads the PC RSA key pair from the named Windows key container.
        /// If the container doesn't exist yet, generates a new 4096-bit key pair
        /// and persists it automatically.
        /// </summary>
        public static RSACryptoServiceProvider LoadOrCreatePcKey()
        {
            var cspParams = new CspParameters
            {
                KeyContainerName = ContainerName,
                KeyNumber         = (int)KeyNumber.Exchange,  // AT_KEYEXCHANGE
                Flags             = CspProviderFlags.UseUserProtectedKey  // Prompts on first creation (optional)
                                  | CspProviderFlags.NoPrompt             // Suppress UI on normal load
            };

            // Turn off NoPrompt so Windows can create the container silently on first run
            cspParams.Flags = CspProviderFlags.UseMachineKeyStore;  // Machine store = survives user profile changes

            // RSACryptoServiceProvider with a named container will:
            //   - CREATE the container + key if it doesn't exist
            //   - LOAD the existing key if the container already exists
            var rsa = new RSACryptoServiceProvider(4096, cspParams);
            rsa.PersistKeyInCsp = true;  // Ensure key is kept in the store

            return rsa;
        }

        /// <summary>
        /// Deletes the PC key pair from the key container (use when you want
        /// to force a new key on the next INIT).
        /// </summary>
        public static void DeletePcKey()
        {
            var cspParams = new CspParameters
            {
                KeyContainerName = ContainerName,
                KeyNumber         = (int)KeyNumber.Exchange,
                Flags             = CspProviderFlags.UseMachineKeyStore
            };
            var rsa = new RSACryptoServiceProvider(cspParams);
            rsa.PersistKeyInCsp = false;  // Setting false then disposing deletes it
            rsa.Clear();
        }

        // ─── Terminal Public Key ──────────────────────────────────────────────────────

        /// <summary>
        /// Saves the terminal's RSA public key (as B64 SubjectPublicKeyInfo) to disk.
        /// </summary>
        public static void SaveTerminalPublicKey(string b64SubjectPublicKeyInfo)
        {
            if (string.IsNullOrEmpty(b64SubjectPublicKeyInfo))
                throw new ArgumentException("Terminal public key must not be empty.");

            File.WriteAllText(TerminalKeyPath, b64SubjectPublicKeyInfo, Encoding.UTF8);
        }

        /// <summary>
        /// Loads the terminal's RSA public key from disk and returns an
        /// RSACryptoServiceProvider configured for encryption only (public key).
        /// Returns null if the file does not exist.
        /// </summary>
        public static RSACryptoServiceProvider LoadTerminalPublicKey()
        {
            if (!File.Exists(TerminalKeyPath))
                return null;

            string b64 = File.ReadAllText(TerminalKeyPath, Encoding.UTF8).Trim();
            if (string.IsNullOrEmpty(b64))
                return null;

            byte[] der = Convert.FromBase64String(b64);

            // Import as SubjectPublicKeyInfo (X.509 DER format)
            var rsa = new RSACryptoServiceProvider();
            rsa.ImportSubjectPublicKeyInfo(der, out _);
            return rsa;
        }

        /// <summary>
        /// Returns true if the terminal key file exists on disk.
        /// </summary>
        public static bool TerminalKeyExists() => File.Exists(TerminalKeyPath);

        /// <summary>
        /// Deletes the saved terminal key from disk.
        /// </summary>
        public static void DeleteTerminalKey()
        {
            if (File.Exists(TerminalKeyPath))
                File.Delete(TerminalKeyPath);
        }
    }
}

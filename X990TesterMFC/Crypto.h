#pragma once
#include <bcrypt.h>
#include <string>
#include <vector>
#include <wincrypt.h>
#include <windows.h>
#include <ncrypt.h>


#pragma comment(lib, "bcrypt.lib")
#pragma comment(lib, "ncrypt.lib")
#pragma comment(lib, "crypt32.lib")

#ifndef NT_SUCCESS
#define NT_SUCCESS(Status) (((NTSTATUS)(Status)) >= 0)
#endif

class CCrypto {
public:
  CCrypto();
  ~CCrypto();

  // Initialize CNG Providers and Generate Ephemeral RSA Key Pair
  bool Initialize();

  // Get Local Public Key in Base64 X.509 SubjectPublicKeyInfo format
  std::string GetPcPublicKey();

  // Set Remote Public Key from Base64 X.509 SubjectPublicKeyInfo format
  bool SetTerminalPublicKey(const std::string &b64Key);

  // Save/Load Terminal Public Key to/from disk
  bool SaveTerminalPublicKey(const std::string &b64Key);
  bool LoadTerminalPublicKey();

  // Check if we have a valid key pair
  bool HasPcKey();

  // Encrypt data (Hybrid: AES-256-ECB for data, RSA-4096 for AES key)
  bool Encrypt(const std::string &plainText, std::string &outEncryptedAesKey,
               std::string &outEncryptedData);

  // Decrypt data (Hybrid: RSA-4096 for AES key, AES-256-ECB for data)
  std::string Decrypt(const std::string &encAesKey, const std::string &encData);

private:
  NCRYPT_PROV_HANDLE m_hProv;
  NCRYPT_KEY_HANDLE m_hPcKey;       // Our private/public key pair
  BCRYPT_KEY_HANDLE m_hTerminalKey; // Terminal's public key
  BCRYPT_ALG_HANDLE m_hAesAlg;

  // Helpers
  std::string Base64Encode(const std::vector<unsigned char> &data);
  std::vector<unsigned char> Base64Decode(const std::string &data);

  // Helper to export CNG Key to X.509
  std::vector<unsigned char> ExportPublicKeyToX509(NCRYPT_KEY_HANDLE hKey);
  // Helper to import X.509 to CNG Key
  BCRYPT_KEY_HANDLE
  ImportPublicKeyFromX509(const std::vector<unsigned char> &x509Data);
};

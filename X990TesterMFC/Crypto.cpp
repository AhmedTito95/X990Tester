#include "pch.h"
#include <algorithm>
#include <fstream>
#include <iostream>
#include <vector>
#include "Crypto.h"
#include "FileLogService.h"

using namespace std;

// CNG works with Big Endian for RSA, so we often don't need the reversals
// required by CAPI.

CCrypto::CCrypto()
    : m_hProv(NULL), m_hPcKey(NULL), m_hTerminalKey(NULL), m_hAesAlg(NULL) {}

CCrypto::~CCrypto() {
  if (m_hPcKey)
    NCryptFreeObject(m_hPcKey);
  if (m_hTerminalKey)
    BCryptDestroyKey(m_hTerminalKey);
  if (m_hProv)
    NCryptFreeObject(m_hProv);
  if (m_hAesAlg)
    BCryptCloseAlgorithmProvider(m_hAesAlg, 0);
}

// Initialize CNG Providers and generate new ephemeral RSA key
bool CCrypto::Initialize() {
  NTSTATUS status;

  // Open KSP
  if (m_hProv)
    NCryptFreeObject(m_hProv);
  status = NCryptOpenStorageProvider(&m_hProv, MS_KEY_STORAGE_PROVIDER, 0);
  if (status != ERROR_SUCCESS) {
    CFileLogService::Log("Crypto", "Failed to open NCrypt storage provider.");
    return false;
  }

  // Open AES Algorithm
  if (m_hAesAlg)
    BCryptCloseAlgorithmProvider(m_hAesAlg, 0);
  status = BCryptOpenAlgorithmProvider(&m_hAesAlg, BCRYPT_AES_ALGORITHM,
                                       MS_PRIMITIVE_PROVIDER, 0);
  if (!NT_SUCCESS(status)) {
    CFileLogService::Log("Crypto", "Failed to open AES algorithm provider.");
    return false;
  }

  // Set AES chaining mode to ECB (to match legacy implementation)
  status = BCryptSetProperty(m_hAesAlg, BCRYPT_CHAINING_MODE,
                             (PUCHAR)BCRYPT_CHAIN_MODE_ECB,
                             sizeof(BCRYPT_CHAIN_MODE_ECB), 0);
  if (!NT_SUCCESS(status)) {
    CFileLogService::Log("Crypto", "Failed to set AES mode to ECB.");
    return false;
  }

  // Open or Create Persistent RSA Key Pair (4096 bits)
  if (m_hPcKey) {
    NCryptFreeObject(m_hPcKey);
    m_hPcKey = NULL;
  }

  status = NCryptOpenKey(m_hProv, &m_hPcKey, L"X990TesterKeyContainer", 0, NCRYPT_MACHINE_KEY_FLAG);
  if (status != ERROR_SUCCESS) {
    CFileLogService::Log("Crypto", "Persistent key not found, creating new one...");
    status = NCryptCreatePersistedKey(m_hProv, &m_hPcKey, NCRYPT_RSA_ALGORITHM, L"X990TesterKeyContainer", 0, NCRYPT_MACHINE_KEY_FLAG);
    if (status != ERROR_SUCCESS) {
      char msg[128];
      sprintf_s(msg, "Failed to create persistent RSA key. Status: 0x%X", status);
      CFileLogService::Log("Crypto", msg);
      return false;
    }
    DWORD keyLen = 4096;
    NCryptSetProperty(m_hPcKey, NCRYPT_LENGTH_PROPERTY, (PBYTE)&keyLen, sizeof(DWORD), 0);
    status = NCryptFinalizeKey(m_hPcKey, 0);
    if (status != ERROR_SUCCESS) {
      CFileLogService::Log("Crypto", "Failed to finalize RSA key.");
      return false;
    }
  }

  if (m_hTerminalKey) {
    BCryptDestroyKey(m_hTerminalKey);
    m_hTerminalKey = NULL; // Clear any old terminal key
  }

  return true;
}

bool CCrypto::HasPcKey() { return (m_hPcKey != NULL); }

bool CCrypto::SaveTerminalPublicKey(const std::string &b64Key) {
  // Simple file storage for the public key string
  if (b64Key.empty())
    return false;

  char buffer[MAX_PATH];
  if (GetModuleFileNameA(NULL, buffer, MAX_PATH) == 0)
    return false;
  std::string path(buffer);
  std::string::size_type pos = path.find_last_of("\\/");
  std::string terminalKeyPath = path.substr(0, pos) + "\\terminal.key";

  std::ofstream outfile(terminalKeyPath);
  if (!outfile.is_open())
    return false;
  outfile << b64Key;
  outfile.close();

  return SetTerminalPublicKey(b64Key);
}

bool CCrypto::LoadTerminalPublicKey() {
  char buffer[MAX_PATH];
  if (GetModuleFileNameA(NULL, buffer, MAX_PATH) == 0)
    return false;
  std::string path(buffer);
  std::string::size_type pos = path.find_last_of("\\/");
  std::string terminalKeyPath = path.substr(0, pos) + "\\terminal.key";

  std::ifstream infile(terminalKeyPath);
  if (!infile.is_open())
    return false;
  std::string b64Key; 
  infile >> b64Key;
  infile.close();

  if (b64Key.empty())
    return false;

  // Strip UTF-8 BOM if present (EF BB BF)
  if (b64Key.size() >= 3 && 
      (unsigned char)b64Key[0] == 0xEF && 
      (unsigned char)b64Key[1] == 0xBB && 
      (unsigned char)b64Key[2] == 0xBF) {
      b64Key.erase(0, 3);
  }

  return SetTerminalPublicKey(b64Key);
}

// --------------------------------------------------------------------------------------
// ENCRYPTION / DECRYPTION
// --------------------------------------------------------------------------------------

bool CCrypto::Encrypt(const std::string &plainText,
                      std::string &outEncryptedAesKey,
                      std::string &outEncryptedData) {
  if (!m_hAesAlg || !m_hTerminalKey)
    return false;

  NTSTATUS status;

  // 1. Generate Random AES Data (32 bytes for AES-256)
  std::vector<unsigned char> aesKey(32);
  status = BCryptGenRandom(NULL, aesKey.data(), (ULONG)aesKey.size(),
                           BCRYPT_USE_SYSTEM_PREFERRED_RNG);
  if (!NT_SUCCESS(status))
    return false;

  // 2. Encrypt Data with AES (ECB with Padding)
  // Create ephemeral key object
  BCRYPT_KEY_HANDLE hAesKey = NULL;
  DWORD cbKeyObject = 0;
  DWORD cbData = 0;
  BCryptGetProperty(m_hAesAlg, BCRYPT_OBJECT_LENGTH, (PUCHAR)&cbKeyObject,
                    sizeof(DWORD), &cbData, 0);
  std::vector<unsigned char> pbKeyObject(cbKeyObject);

  status = BCryptGenerateSymmetricKey(m_hAesAlg, &hAesKey, pbKeyObject.data(),
                                      cbKeyObject, aesKey.data(),
                                      (ULONG)aesKey.size(), 0);
  if (!NT_SUCCESS(status))
    return false;

  // Determine output size (with padding)
  std::vector<unsigned char> plainBuf(plainText.begin(), plainText.end());
  ULONG cbResult = 0;
  // BCRYPT_BLOCK_PADDING is equivalent to PKCS#7
  status = BCryptEncrypt(hAesKey, plainBuf.data(), (ULONG)plainBuf.size(), NULL,
                         NULL, 0, NULL, 0, &cbResult, BCRYPT_BLOCK_PADDING);
  if (!NT_SUCCESS(status)) {
    BCryptDestroyKey(hAesKey);
    return false;
  }

  std::vector<unsigned char> cipherBuf(cbResult);
  status = BCryptEncrypt(hAesKey, plainBuf.data(), (ULONG)plainBuf.size(), NULL,
                         NULL, 0, cipherBuf.data(), cbResult, &cbResult,
                         BCRYPT_BLOCK_PADDING);
  BCryptDestroyKey(hAesKey); // Done with AES handle
  if (!NT_SUCCESS(status))
    return false;

  outEncryptedData = Base64Encode(cipherBuf);

  // 3. Encrypt AES Key with RSA (Terminal Public Key)
  // CNG RSA Encrypt uses PKCS1 padding by default if BCRYPT_PAD_PKCS1 is
  // specified. CNG Output is already Big Endian (Standard), so NO REVERSAL
  // needed for .NET/Java compatibility.

  // Calculate RSA output size
  status = BCryptEncrypt(m_hTerminalKey, aesKey.data(), (ULONG)aesKey.size(),
                         NULL, NULL, 0, NULL, 0, &cbResult, BCRYPT_PAD_PKCS1);
  if (!NT_SUCCESS(status))
    return false;

  std::vector<unsigned char> encryptedKeyBuf(cbResult);
  status = BCryptEncrypt(m_hTerminalKey, aesKey.data(), (ULONG)aesKey.size(),
                         NULL, NULL, 0, encryptedKeyBuf.data(), cbResult,
                         &cbResult, BCRYPT_PAD_PKCS1);
  if (!NT_SUCCESS(status))
    return false;

  outEncryptedAesKey = Base64Encode(encryptedKeyBuf);

  return true;
}

std::string CCrypto::Decrypt(const std::string &encAesKey,
                             const std::string &encData) {
  if (!m_hPcKey || !m_hAesAlg) {
    CFileLogService::Log("Decrypt", "Keys not initialized.");
    return "";
  }

  std::vector<unsigned char> encKeyBuf = Base64Decode(encAesKey);
  std::vector<unsigned char> encDataBuf = Base64Decode(encData);
  if (encKeyBuf.empty() || encDataBuf.empty())
    return "";

  NTSTATUS status;
  ULONG cbResult = 0;

  // 1. Decrypt AES Key with RSA (Private Key)
  // Input is Big Endian (from network). CNG expects Big Endian. No Reversal Needed.
  DWORD keyLenBits = 0;
  DWORD cbData = 0;
  NCryptGetProperty(m_hPcKey, NCRYPT_LENGTH_PROPERTY, (PUCHAR)&keyLenBits,
                    sizeof(DWORD), &cbData, 0);
  DWORD keyBytes = keyLenBits / 8; // Used to pre-allocate NCryptDecrypt output buffer

  // NCryptDecrypt with RSA/PKCS1 does NOT support the two-call pattern
  // (NULL output buffer to query size) — it returns NTE_INVALID_PARAMETER.
  // The decrypted output is always <= key size in bytes, so pre-allocate that.
  std::vector<unsigned char> rawAesKey(keyBytes);
  cbResult = keyBytes;
  status = NCryptDecrypt(m_hPcKey, encKeyBuf.data(), (DWORD)encKeyBuf.size(),
                         NULL, rawAesKey.data(), cbResult, &cbResult,
                         NCRYPT_PAD_PKCS1_FLAG);
  if (!NT_SUCCESS(status)) {
    char msg[128];
    sprintf_s(msg, "Error decrypting AES key (RSA). Status: 0x%X", status);
    CFileLogService::Log("Decrypt", msg);
    return "";
  }
  // Trim to the actual decrypted size (PKCS1 removes padding, so cbResult = 32 for AES-256)
  rawAesKey.resize(cbResult);
  // rawAesKey now contains the 32-byte AES key

  // 2. Decrypt Data with AES
  // Import the raw AES key
  BCRYPT_KEY_HANDLE hAesKey = NULL;
  DWORD cbKeyObject = 0;
  BCryptGetProperty(m_hAesAlg, BCRYPT_OBJECT_LENGTH, (PUCHAR)&cbKeyObject,
                    sizeof(DWORD), &cbData, 0);
  std::vector<unsigned char> pbKeyObject(cbKeyObject);

  status = BCryptGenerateSymmetricKey(m_hAesAlg, &hAesKey, pbKeyObject.data(),
                                      cbKeyObject, rawAesKey.data(),
                                      (ULONG)rawAesKey.size(), 0);
  if (!NT_SUCCESS(status)) {
    CFileLogService::Log("Decrypt",
                         "Failed to generate symmetric key from raw bytes.");
    return "";
  }

  status =
      BCryptDecrypt(hAesKey, encDataBuf.data(), (ULONG)encDataBuf.size(), NULL,
                    NULL, 0, NULL, 0, &cbResult, BCRYPT_BLOCK_PADDING);
  if (!NT_SUCCESS(status)) {
    char msg[128];
    sprintf_s(msg, "Error decrypting Data (AES). Status: 0x%X", status);
    CFileLogService::Log("Decrypt", msg);
    BCryptDestroyKey(hAesKey);
    return "";
  }

  std::vector<unsigned char> plainBuf(cbResult);
  status = BCryptDecrypt(hAesKey, encDataBuf.data(), (ULONG)encDataBuf.size(),
                         NULL, NULL, 0, plainBuf.data(), cbResult, &cbResult,
                         BCRYPT_BLOCK_PADDING);
  BCryptDestroyKey(hAesKey);

  if (!NT_SUCCESS(status))
    return "";

  // Convert to string (trim tail zeros if any, though PKCS7 handles padding
  // removal)
  std::string result(plainBuf.begin(), plainBuf.begin() + cbResult);
  return result;
}

// --------------------------------------------------------------------------------------
// EXPORT / IMPORT HELPERS
// --------------------------------------------------------------------------------------

std::string CCrypto::GetPcPublicKey() {
  if (!m_hPcKey)
    return "";
  std::vector<unsigned char> x509 = ExportPublicKeyToX509(m_hPcKey);
  return Base64Encode(x509);
}

std::vector<unsigned char>
CCrypto::ExportPublicKeyToX509(NCRYPT_KEY_HANDLE hKey) {
  // 1. Export CNG Private Key to LEGACY_RSAPUBLIC_BLOB (compatible with CAPI)
  // This allows us to use CryptEncodeObject without manually building ASN.1
  NTSTATUS status;
  ULONG cbBlob = 0;
  status =
      NCryptExportKey(hKey, NULL, LEGACY_RSAPUBLIC_BLOB, NULL, NULL, 0, &cbBlob, 0);
  if (!NT_SUCCESS(status))
    return {};

  std::vector<unsigned char> blob(cbBlob);
  status = NCryptExportKey(hKey, NULL, LEGACY_RSAPUBLIC_BLOB, NULL, blob.data(),
                           cbBlob, &cbBlob, 0);
  if (!NT_SUCCESS(status))
    return {};

  // 2. Encode to X.509 using CAPI
  // Step A: Convert LEGACY Blob to DER-Encoded RSA Public Key
  // Note: The helper CryptEncodeObject/RSA_CSP_PUBLICKEYBLOB expects the struct
  // PUBLICKEYSTRUC+RSAPUBKEY which is exactly what LEGACY_RSAPUBLIC_BLOB is.

  DWORD cbDer = 0;
  if (!CryptEncodeObject(X509_ASN_ENCODING, RSA_CSP_PUBLICKEYBLOB, blob.data(),
                         NULL, &cbDer)) {
    return {};
  }
  std::vector<unsigned char> derKey(cbDer);
  if (!CryptEncodeObject(X509_ASN_ENCODING, RSA_CSP_PUBLICKEYBLOB, blob.data(),
                         derKey.data(), &cbDer)) {
    return {};
  }

  // Step B: Wrap in X.509 SubjectPublicKeyInfo
  CERT_PUBLIC_KEY_INFO pubKeyInfo;
  pubKeyInfo.Algorithm.pszObjId = (LPSTR)szOID_RSA_RSA;
  pubKeyInfo.Algorithm.Parameters.cbData = 0;
  pubKeyInfo.Algorithm.Parameters.pbData = NULL;
  pubKeyInfo.PublicKey.cbData = cbDer;
  pubKeyInfo.PublicKey.pbData = derKey.data();
  pubKeyInfo.PublicKey.cUnusedBits = 0;

  DWORD cbInfo = 0;
  if (!CryptEncodeObject(X509_ASN_ENCODING, X509_PUBLIC_KEY_INFO, &pubKeyInfo,
                         NULL, &cbInfo)) {
    return {};
  }
  std::vector<unsigned char> x509(cbInfo);
  if (!CryptEncodeObject(X509_ASN_ENCODING, X509_PUBLIC_KEY_INFO, &pubKeyInfo,
                         x509.data(), &cbInfo)) {
    return {};
  }

  return x509;
}

bool CCrypto::SetTerminalPublicKey(const std::string &b64Key) {
  if (m_hTerminalKey) {
    BCryptDestroyKey(m_hTerminalKey);
    m_hTerminalKey = NULL;
  }

  std::vector<unsigned char> x509 = Base64Decode(b64Key);
  if (x509.empty())
    return false;

  m_hTerminalKey = ImportPublicKeyFromX509(x509);
  if (m_hTerminalKey != NULL) {
    CFileLogService::Log("Crypto",
                         "SetTerminalPublicKey: Key Imported Successfully.");
    return true;
  }
  return false;
}

BCRYPT_KEY_HANDLE
CCrypto::ImportPublicKeyFromX509(const std::vector<unsigned char> &x509Data) {
  // 1. Decode X.509 to CERT_PUBLIC_KEY_INFO
  DWORD cbInfo = 0;
  if (!CryptDecodeObject(X509_ASN_ENCODING, X509_PUBLIC_KEY_INFO,
                         x509Data.data(), (DWORD)x509Data.size(), 0, NULL,
                         &cbInfo)) {
    return NULL;
  }
  std::vector<unsigned char> infoBuf(cbInfo);
  PCERT_PUBLIC_KEY_INFO pInfo = (PCERT_PUBLIC_KEY_INFO)infoBuf.data();
  if (!CryptDecodeObject(X509_ASN_ENCODING, X509_PUBLIC_KEY_INFO,
                         x509Data.data(), (DWORD)x509Data.size(), 0, pInfo,
                         &cbInfo)) {
    return NULL;
  }

  // 2. Import using CryptImportPublicKeyInfoEx2 (Valid since Vista, handles CNG
  // internally)
  BCRYPT_KEY_HANDLE hKey = NULL;
  // this function is in crypt32.lib and handles the OID -> CNG Provider mapping
  // automatically
  if (!CryptImportPublicKeyInfoEx2(X509_ASN_ENCODING, pInfo, 0, NULL, &hKey)) {
    DWORD err = GetLastError();
    char msg[128];
    sprintf_s(msg,
              "Failed to import Terminal Key via CryptImportPublicKeyInfoEx2. "
              "Error: 0x%X",
              err);
    CFileLogService::Log("Crypto", msg);
    return NULL;
  }

  return hKey;
}

// --------------------------------------------------------------------------------------
// UTILS
// --------------------------------------------------------------------------------------

std::string CCrypto::Base64Encode(const std::vector<unsigned char> &data) {
  DWORD dwLen = 0;
  if (!CryptBinaryToStringA(data.data(), (DWORD)data.size(),
                            CRYPT_STRING_BASE64 | CRYPT_STRING_NOCRLF, NULL,
                            &dwLen))
    return "";

  std::string str(dwLen, '\0');
  if (!CryptBinaryToStringA(data.data(), (DWORD)data.size(),
                            CRYPT_STRING_BASE64 | CRYPT_STRING_NOCRLF, &str[0],
                            &dwLen))
    return "";

  if (!str.empty() && str.back() == '\0')
    str.pop_back();
  while (!str.empty() &&
         (str.back() == '\r' || str.back() == '\n' || str.back() == '\0'))
    str.pop_back();

  return str;
}

std::vector<unsigned char> CCrypto::Base64Decode(const std::string &data) {
  DWORD dwLen = 0;
  if (!CryptStringToBinaryA(data.c_str(), (DWORD)data.length(),
                            CRYPT_STRING_BASE64, NULL, &dwLen, NULL, NULL))
    return {};

  std::vector<unsigned char> bin(dwLen);
  if (!CryptStringToBinaryA(data.c_str(), (DWORD)data.length(),
                            CRYPT_STRING_BASE64, bin.data(), &dwLen, NULL,
                            NULL))
    return {};

  return bin;
}

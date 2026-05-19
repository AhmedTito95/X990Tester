#include "pch.h"
#include "CryptoService.h"
#include "FileLogService.h"

using namespace std;

bool CCryptoService::Encrypt(CKeyStorageService &keys,
                             const std::string &plainText,
                             CString &outEncryptedAesKey,
                             CString &outEncryptedData) {
  BCRYPT_KEY_HANDLE hTerminalKey = keys.GetTerminalKeyHandle();
  if (!hTerminalKey)
    return false;

  NTSTATUS status;

  // Open AES algorithm provider (ephemeral, scoped to this call)
  BCRYPT_ALG_HANDLE hAesAlg = NULL;
  status = BCryptOpenAlgorithmProvider(&hAesAlg, BCRYPT_AES_ALGORITHM,
                                       MS_PRIMITIVE_PROVIDER, 0);
  if (!NT_SUCCESS(status))
    return false;

  // Set AES chaining mode to ECB
  status = BCryptSetProperty(hAesAlg, BCRYPT_CHAINING_MODE,
                             (PUCHAR)BCRYPT_CHAIN_MODE_ECB,
                             sizeof(BCRYPT_CHAIN_MODE_ECB), 0);
  if (!NT_SUCCESS(status)) {
    BCryptCloseAlgorithmProvider(hAesAlg, 0);
    return false;
  }

  // 1. Generate Random AES Key (32 bytes for AES-256)
  std::vector<unsigned char> aesKey(32);
  status = BCryptGenRandom(NULL, aesKey.data(), (ULONG)aesKey.size(),
                           BCRYPT_USE_SYSTEM_PREFERRED_RNG);
  if (!NT_SUCCESS(status)) {
    BCryptCloseAlgorithmProvider(hAesAlg, 0);
    return false;
  }

  // 2. Encrypt Data with AES (ECB with PKCS7 Padding)
  BCRYPT_KEY_HANDLE hAesKey = NULL;
  DWORD cbKeyObject = 0;
  DWORD cbData = 0;
  BCryptGetProperty(hAesAlg, BCRYPT_OBJECT_LENGTH, (PUCHAR)&cbKeyObject,
                    sizeof(DWORD), &cbData, 0);
  std::vector<unsigned char> pbKeyObject(cbKeyObject);

  status = BCryptGenerateSymmetricKey(hAesAlg, &hAesKey, pbKeyObject.data(),
                                      cbKeyObject, aesKey.data(),
                                      (ULONG)aesKey.size(), 0);
  if (!NT_SUCCESS(status)) {
    BCryptCloseAlgorithmProvider(hAesAlg, 0);
    return false;
  }

  // Determine output size (with padding)
  std::vector<unsigned char> plainBuf(plainText.begin(), plainText.end());
  ULONG cbResult = 0;
  status = BCryptEncrypt(hAesKey, plainBuf.data(), (ULONG)plainBuf.size(), NULL,
                         NULL, 0, NULL, 0, &cbResult, BCRYPT_BLOCK_PADDING);
  if (!NT_SUCCESS(status)) {
    BCryptDestroyKey(hAesKey);
    BCryptCloseAlgorithmProvider(hAesAlg, 0);
    return false;
  }

  std::vector<unsigned char> cipherBuf(cbResult);
  status = BCryptEncrypt(hAesKey, plainBuf.data(), (ULONG)plainBuf.size(), NULL,
                         NULL, 0, cipherBuf.data(), cbResult, &cbResult,
                         BCRYPT_BLOCK_PADDING);
  BCryptDestroyKey(hAesKey);
  BCryptCloseAlgorithmProvider(hAesAlg, 0);
  if (!NT_SUCCESS(status))
    return false;

  outEncryptedData = Base64Encode(cipherBuf);

  // 3. Encrypt AES Key with RSA (Terminal Public Key, PKCS1 padding)
  status = BCryptEncrypt(hTerminalKey, aesKey.data(), (ULONG)aesKey.size(),
                         NULL, NULL, 0, NULL, 0, &cbResult, BCRYPT_PAD_PKCS1);
  if (!NT_SUCCESS(status))
    return false;

  std::vector<unsigned char> encryptedKeyBuf(cbResult);
  status = BCryptEncrypt(hTerminalKey, aesKey.data(), (ULONG)aesKey.size(),
                         NULL, NULL, 0, encryptedKeyBuf.data(), cbResult,
                         &cbResult, BCRYPT_PAD_PKCS1);
  if (!NT_SUCCESS(status))
    return false;

  outEncryptedAesKey = Base64Encode(encryptedKeyBuf);

  return true;
}

std::string CCryptoService::Decrypt(CKeyStorageService &keys,
                                    const std::string &encAesKey,
                                    const std::string &encData) {
  NCRYPT_KEY_HANDLE hPcKey = keys.GetPcKeyHandle();
  if (!hPcKey) {
    CFileLogService::Log("Decrypt", "PC key not initialized.");
    return "";
  }

  std::vector<unsigned char> encKeyBuf = Base64Decode(encAesKey);
  std::vector<unsigned char> encDataBuf = Base64Decode(encData);
  if (encKeyBuf.empty() || encDataBuf.empty())
    return "";

  NTSTATUS status;
  ULONG cbResult = 0;

  // 1. Decrypt AES Key with RSA (Private Key)
  DWORD keyLenBits = 0;
  DWORD cbData = 0;
  NCryptGetProperty(hPcKey, NCRYPT_LENGTH_PROPERTY, (PUCHAR)&keyLenBits,
                    sizeof(DWORD), &cbData, 0);
  DWORD keyBytes = keyLenBits / 8;

  std::vector<unsigned char> rawAesKey(keyBytes);
  cbResult = keyBytes;
  status = NCryptDecrypt(hPcKey, encKeyBuf.data(), (DWORD)encKeyBuf.size(),
                         NULL, rawAesKey.data(), cbResult, &cbResult,
                         NCRYPT_PAD_PKCS1_FLAG);
  if (!NT_SUCCESS(status)) {
    char msg[128];
    sprintf_s(msg, "Error decrypting AES key (RSA). Status: 0x%X", status);
    CFileLogService::Log("Decrypt", msg);
    return "";
  }
  rawAesKey.resize(cbResult);

  // 2. Decrypt Data with AES
  BCRYPT_ALG_HANDLE hAesAlg = NULL;
  status = BCryptOpenAlgorithmProvider(&hAesAlg, BCRYPT_AES_ALGORITHM,
                                       MS_PRIMITIVE_PROVIDER, 0);
  if (!NT_SUCCESS(status))
    return "";

  status = BCryptSetProperty(hAesAlg, BCRYPT_CHAINING_MODE,
                             (PUCHAR)BCRYPT_CHAIN_MODE_ECB,
                             sizeof(BCRYPT_CHAIN_MODE_ECB), 0);
  if (!NT_SUCCESS(status)) {
    BCryptCloseAlgorithmProvider(hAesAlg, 0);
    return "";
  }

  BCRYPT_KEY_HANDLE hAesKey = NULL;
  DWORD cbKeyObject = 0;
  BCryptGetProperty(hAesAlg, BCRYPT_OBJECT_LENGTH, (PUCHAR)&cbKeyObject,
                    sizeof(DWORD), &cbData, 0);
  std::vector<unsigned char> pbKeyObject(cbKeyObject);

  status = BCryptGenerateSymmetricKey(hAesAlg, &hAesKey, pbKeyObject.data(),
                                      cbKeyObject, rawAesKey.data(),
                                      (ULONG)rawAesKey.size(), 0);
  if (!NT_SUCCESS(status)) {
    CFileLogService::Log("Decrypt",
                         "Failed to generate symmetric key from raw bytes.");
    BCryptCloseAlgorithmProvider(hAesAlg, 0);
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
    BCryptCloseAlgorithmProvider(hAesAlg, 0);
    return "";
  }

  std::vector<unsigned char> plainBuf(cbResult);
  status = BCryptDecrypt(hAesKey, encDataBuf.data(), (ULONG)encDataBuf.size(),
                         NULL, NULL, 0, plainBuf.data(), cbResult, &cbResult,
                         BCRYPT_BLOCK_PADDING);
  BCryptDestroyKey(hAesKey);
  BCryptCloseAlgorithmProvider(hAesAlg, 0);

  if (!NT_SUCCESS(status))
    return "";

  std::string result(plainBuf.begin(), plainBuf.begin() + cbResult);
  return result;
}

// --------------------------------------------------------------------------------------
// UTILS
// --------------------------------------------------------------------------------------

std::string
CCryptoService::Base64Encode(const std::vector<unsigned char> &data) {
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

std::vector<unsigned char>
CCryptoService::Base64Decode(const std::string &data) {
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

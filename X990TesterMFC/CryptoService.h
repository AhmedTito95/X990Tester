#pragma once
#include "KeyStorageService.h"
#include <string>

/// Stateless encryption/decryption service.
/// Uses key handles from CKeyStorageService to perform hybrid
/// RSA + AES-256-ECB encryption and decryption.
class CCryptoService {
public:
  // Encrypt plainText using hybrid encryption:
  //   - AES-256-ECB for the data (random key each call)
  //   - RSA (Terminal public key) for the AES key
  // Returns Base64-encoded encrypted key and data via out params.
  static bool Encrypt(CKeyStorageService &keys, const std::string &plainText,
                      std::string &outEncryptedAesKey,
                      std::string &outEncryptedData);

  // Decrypt using hybrid decryption:
  //   - RSA (PC private key) for the AES key
  //   - AES-256-ECB for the data
  // Returns the decrypted plaintext string, or empty on failure.
  static std::string Decrypt(CKeyStorageService &keys,
                             const std::string &encAesKey,
                             const std::string &encData);

private:
  // Base64 helpers
  static std::string Base64Encode(const std::vector<unsigned char> &data);
  static std::vector<unsigned char> Base64Decode(const std::string &data);
};

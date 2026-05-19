#pragma once
#include <bcrypt.h>
#include <ncrypt.h>
#include <vector>
#include <wincrypt.h>
#include <windows.h>

#pragma comment(lib, "bcrypt.lib")
#pragma comment(lib, "ncrypt.lib")
#pragma comment(lib, "crypt32.lib")

#ifndef NT_SUCCESS
#define NT_SUCCESS(Status) (((NTSTATUS)(Status)) >= 0)
#endif

/// Manages secure persistence of the PC RSA key pair and the Terminal public key.
///
/// Strategy:
///   - PC RSA Key Pair:  Stored in the Windows CNG Key Store under a named
///                       container ("X990TesterKeyContainer"). The private
///                       key never leaves the OS key store in plain text.
///   - Terminal Public Key: Stored as a Base64 string in the Windows Registry
///                          under HKLM\SOFTWARE\X990Tester (value "TerminalPublicKey").
class CKeyStorageServiceReg {
public:
  CKeyStorageServiceReg();
  ~CKeyStorageServiceReg();

  // Load or create the PC RSA key pair from the named Windows key container.
  // If the container doesn't exist, generates a new 4096-bit key pair.
  bool LoadOrCreatePcKey();

  // Get the PC private key handle (for decryption)
  NCRYPT_KEY_HANDLE GetPcKeyHandle() const;

  // Get the PC public key in Base64 X.509 SubjectPublicKeyInfo format
  CString GetPcPublicKeyBase64();

  // Check if we have a valid PC key
  bool HasPcKey() const;

  // Save the terminal's public key (Base64 X.509) to the registry
  bool SaveTerminalPublicKey(const CString &b64Key);

  // Load the terminal's public key from the registry and import it
  bool LoadTerminalPublicKey();

  // Set the terminal's public key from Base64 X.509 (in-memory only)
  bool SetTerminalPublicKey(const CString &b64Key);

  // Get the terminal public key handle (for encryption)
  BCRYPT_KEY_HANDLE GetTerminalKeyHandle() const;

  // Check if we have a valid terminal key
  bool HasTerminalKey() const;

private:
  static constexpr const wchar_t *ContainerName = L"X990TesterKeyContainer";

  // Registry location for the terminal public key
  static constexpr LPCTSTR RegistrySubKey = _T("SOFTWARE\\X990Tester");
  static constexpr LPCTSTR RegistryValueName = _T("TerminalPublicKey");

  NCRYPT_PROV_HANDLE m_hProv;
  NCRYPT_KEY_HANDLE m_hPcKey;
  BCRYPT_KEY_HANDLE m_hTerminalKey;

  // Helpers
  CString Base64Encode(const std::vector<unsigned char> &data);
  std::vector<unsigned char> Base64Decode(const CString &data);
  std::vector<unsigned char> ExportPublicKeyToX509(NCRYPT_KEY_HANDLE hKey);
  BCRYPT_KEY_HANDLE
  ImportPublicKeyFromX509(const std::vector<unsigned char> &x509Data);
};

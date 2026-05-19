#include "pch.h"
#include "KeyStorageServiceReg.h"
#include "FileLogService.h"
#include <algorithm>

CKeyStorageServiceReg::CKeyStorageServiceReg()
    : m_hProv(NULL), m_hPcKey(NULL), m_hTerminalKey(NULL) {}

CKeyStorageServiceReg::~CKeyStorageServiceReg() {
  if (m_hPcKey)
    NCryptFreeObject(m_hPcKey);
  if (m_hTerminalKey)
    BCryptDestroyKey(m_hTerminalKey);
  if (m_hProv)
    NCryptFreeObject(m_hProv);
}

bool CKeyStorageServiceReg::LoadOrCreatePcKey() {
  NTSTATUS status;

  // Open KSP
  if (m_hProv)
    NCryptFreeObject(m_hProv);
  status = NCryptOpenStorageProvider(&m_hProv, MS_KEY_STORAGE_PROVIDER, 0);
  if (status != ERROR_SUCCESS) {
    CFileLogService::Log("KeyStorage", "Failed to open NCrypt storage provider.");
    return false;
  }

  // Open or Create Persistent RSA Key Pair (4096 bits)
  if (m_hPcKey) {
    NCryptFreeObject(m_hPcKey);
    m_hPcKey = NULL;
  }

  status = NCryptOpenKey(m_hProv, &m_hPcKey, ContainerName, 0,
                         NCRYPT_MACHINE_KEY_FLAG);
  if (status != ERROR_SUCCESS) {
    CFileLogService::Log("KeyStorage",
                         "Persistent key not found, creating new one...");
    status = NCryptCreatePersistedKey(m_hProv, &m_hPcKey, NCRYPT_RSA_ALGORITHM,
                                      ContainerName, 0,
                                      NCRYPT_MACHINE_KEY_FLAG);
    if (status != ERROR_SUCCESS) {
      CString msg;
      msg.Format(_T("Failed to create persistent RSA key. Status: 0x%X"),
                 status);
      CFileLogService::Log("KeyStorage", CStringA(msg));
      return false;
    }
    DWORD keyLen = 4096;
    NCryptSetProperty(m_hPcKey, NCRYPT_LENGTH_PROPERTY, (PBYTE)&keyLen,
                      sizeof(DWORD), 0);
    status = NCryptFinalizeKey(m_hPcKey, 0);
    if (status != ERROR_SUCCESS) {
      CFileLogService::Log("KeyStorage", "Failed to finalize RSA key.");
      return false;
    }
  }

  return true;
}

NCRYPT_KEY_HANDLE CKeyStorageServiceReg::GetPcKeyHandle() const {
  return m_hPcKey;
}

bool CKeyStorageServiceReg::HasPcKey() const { return (m_hPcKey != NULL); }

BCRYPT_KEY_HANDLE CKeyStorageServiceReg::GetTerminalKeyHandle() const {
  return m_hTerminalKey;
}

bool CKeyStorageServiceReg::HasTerminalKey() const {
  return (m_hTerminalKey != NULL);
}

CString CKeyStorageServiceReg::GetPcPublicKeyBase64() {
  if (!m_hPcKey)
    return _T("");
  std::vector<unsigned char> x509 = ExportPublicKeyToX509(m_hPcKey);
  return Base64Encode(x509);
}

bool CKeyStorageServiceReg::SetTerminalPublicKey(const CString &b64Key) {
  if (m_hTerminalKey) {
    BCryptDestroyKey(m_hTerminalKey);
    m_hTerminalKey = NULL;
  }

  std::vector<unsigned char> x509 = Base64Decode(b64Key);
  if (x509.empty())
    return false;

  m_hTerminalKey = ImportPublicKeyFromX509(x509);
  if (m_hTerminalKey != NULL) {
    CFileLogService::Log("KeyStorage",
                         "SetTerminalPublicKey: Key Imported Successfully.");
    return true;
  }
  return false;
}

// ---------------------------------------------------------------------------
// REGISTRY-BASED TERMINAL KEY PERSISTENCE
// ---------------------------------------------------------------------------

bool CKeyStorageServiceReg::SaveTerminalPublicKey(const CString &b64Key) {
  if (b64Key.IsEmpty())
    return false;

  HKEY hKey = NULL;
  LONG result = RegCreateKeyEx(
      HKEY_LOCAL_MACHINE, RegistrySubKey, 0, NULL,
      REG_OPTION_NON_VOLATILE, KEY_WRITE, NULL, &hKey, NULL);

  if (result != ERROR_SUCCESS) {
    CString msg;
    msg.Format(_T("SaveTerminalPublicKey: Failed to open/create registry key ")
               _T("HKLM\\%s. Error: %ld"),
               RegistrySubKey, result);
    CFileLogService::Log("KeyStorage", CStringA(msg));
    return false;
  }

  // Write the Base64 key as a REG_SZ string
  result = RegSetValueEx(
      hKey, RegistryValueName, 0, REG_SZ,
      reinterpret_cast<const BYTE *>(static_cast<LPCTSTR>(b64Key)),
      (b64Key.GetLength() + 1) * sizeof(TCHAR)); // +1 for null terminator

  RegCloseKey(hKey);

  if (result != ERROR_SUCCESS) {
    CString msg;
    msg.Format(_T("SaveTerminalPublicKey: Failed to write registry value '%s'. ")
               _T("Error: %ld"),
               RegistryValueName, result);
    CFileLogService::Log("KeyStorage", CStringA(msg));
    return false;
  }

  CFileLogService::Log("KeyStorage",
                        "SaveTerminalPublicKey: Key saved to registry.");
  return SetTerminalPublicKey(b64Key);
}

bool CKeyStorageServiceReg::LoadTerminalPublicKey() {
  HKEY hKey = NULL;
  LONG result = RegOpenKeyEx(
      HKEY_LOCAL_MACHINE, RegistrySubKey, 0, KEY_READ, &hKey);

  if (result != ERROR_SUCCESS) {
    CFileLogService::Log("KeyStorage",
                         "LoadTerminalPublicKey: Registry key not found.");
    return false;
  }

  // Query the size first
  DWORD dataType = 0;
  DWORD dataSize = 0;
  result = RegQueryValueEx(hKey, RegistryValueName, NULL, &dataType,
                           NULL, &dataSize);

  if (result != ERROR_SUCCESS || dataType != REG_SZ || dataSize == 0) {
    RegCloseKey(hKey);
    CFileLogService::Log("KeyStorage",
                         "LoadTerminalPublicKey: Registry value not found "
                         "or wrong type.");
    return false;
  }

  // Read the value directly into a CString buffer
  DWORD charCount = dataSize / sizeof(TCHAR);
  CString b64Key;
  result = RegQueryValueEx(hKey, RegistryValueName, NULL, NULL,
                           reinterpret_cast<BYTE *>(b64Key.GetBuffer(charCount)),
                           &dataSize);
  b64Key.ReleaseBuffer();
  RegCloseKey(hKey);

  if (result != ERROR_SUCCESS) {
    CFileLogService::Log("KeyStorage",
                         "LoadTerminalPublicKey: Failed to read registry value.");
    return false;
  }

  if (b64Key.IsEmpty())
    return false;

  CFileLogService::Log("KeyStorage",
                        "LoadTerminalPublicKey: Key loaded from registry.");
  return SetTerminalPublicKey(b64Key);
}

// --------------------------------------------------------------------------------------
// EXPORT / IMPORT HELPERS
// --------------------------------------------------------------------------------------

std::vector<unsigned char>
CKeyStorageServiceReg::ExportPublicKeyToX509(NCRYPT_KEY_HANDLE hKey) {
  NTSTATUS status;
  ULONG cbBlob = 0;
  status = NCryptExportKey(hKey, NULL, LEGACY_RSAPUBLIC_BLOB, NULL, NULL, 0,
                           &cbBlob, 0);
  if (!NT_SUCCESS(status))
    return {};

  std::vector<unsigned char> blob(cbBlob);
  status = NCryptExportKey(hKey, NULL, LEGACY_RSAPUBLIC_BLOB, NULL, blob.data(),
                           cbBlob, &cbBlob, 0);
  if (!NT_SUCCESS(status))
    return {};

  // Encode to X.509 using CAPI
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

  // Wrap in X.509 SubjectPublicKeyInfo
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

BCRYPT_KEY_HANDLE CKeyStorageServiceReg::ImportPublicKeyFromX509(
    const std::vector<unsigned char> &x509Data) {
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

  BCRYPT_KEY_HANDLE hKey = NULL;
  if (!CryptImportPublicKeyInfoEx2(X509_ASN_ENCODING, pInfo, 0, NULL, &hKey)) {
    DWORD err = GetLastError();
    CString msg;
    msg.Format(_T("Failed to import Terminal Key via ")
               _T("CryptImportPublicKeyInfoEx2. Error: 0x%X"), err);
    CFileLogService::Log("KeyStorage", CStringA(msg));
    return NULL;
  }

  return hKey;
}

// --------------------------------------------------------------------------------------
// UTILS
// --------------------------------------------------------------------------------------

CString
CKeyStorageServiceReg::Base64Encode(const std::vector<unsigned char> &data) {
  DWORD dwLen = 0;
  if (!CryptBinaryToStringA(data.data(), (DWORD)data.size(),
                            CRYPT_STRING_BASE64 | CRYPT_STRING_NOCRLF, NULL,
                            &dwLen))
    return _T("");

  CStringA strA;
  if (!CryptBinaryToStringA(data.data(), (DWORD)data.size(),
                            CRYPT_STRING_BASE64 | CRYPT_STRING_NOCRLF,
                            strA.GetBuffer(dwLen), &dwLen))
    return _T("");
  strA.ReleaseBuffer();

  // Trim any trailing whitespace
  strA.TrimRight(_T("\r\n"));

  return CString(strA);
}

std::vector<unsigned char>
CKeyStorageServiceReg::Base64Decode(const CString &data) {
  // Base64 is pure ASCII, convert to narrow string for the API
  CStringA strA(data);

  DWORD dwLen = 0;
  if (!CryptStringToBinaryA(strA, strA.GetLength(),
                            CRYPT_STRING_BASE64, NULL, &dwLen, NULL, NULL))
    return {};

  std::vector<unsigned char> bin(dwLen);
  if (!CryptStringToBinaryA(strA, strA.GetLength(),
                            CRYPT_STRING_BASE64, bin.data(), &dwLen, NULL,
                            NULL))
    return {};

  return bin;
}

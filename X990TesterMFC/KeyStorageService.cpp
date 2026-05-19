#include "pch.h"
#include "KeyStorageService.h"
#include "FileLogService.h"
#include <algorithm>
#include <fstream>

using namespace std;

CKeyStorageService::CKeyStorageService()
    : m_hProv(NULL), m_hPcKey(NULL), m_hTerminalKey(NULL) {}

CKeyStorageService::~CKeyStorageService() {
  if (m_hPcKey)
    NCryptFreeObject(m_hPcKey);
  if (m_hTerminalKey)
    BCryptDestroyKey(m_hTerminalKey);
  if (m_hProv)
    NCryptFreeObject(m_hProv);
}

bool CKeyStorageService::LoadOrCreatePcKey() {
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
      char msg[128];
      sprintf_s(msg, "Failed to create persistent RSA key. Status: 0x%X",
                status);
      CFileLogService::Log("KeyStorage", msg);
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

NCRYPT_KEY_HANDLE CKeyStorageService::GetPcKeyHandle() const {
  return m_hPcKey;
}

bool CKeyStorageService::HasPcKey() const { return (m_hPcKey != NULL); }

BCRYPT_KEY_HANDLE CKeyStorageService::GetTerminalKeyHandle() const {
  return m_hTerminalKey;
}

bool CKeyStorageService::HasTerminalKey() const {
  return (m_hTerminalKey != NULL);
}

std::string CKeyStorageService::GetPcPublicKeyBase64() {
  if (!m_hPcKey)
    return "";
  std::vector<unsigned char> x509 = ExportPublicKeyToX509(m_hPcKey);
  return Base64Encode(x509);
}

bool CKeyStorageService::SetTerminalPublicKey(const std::string &b64Key) {
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

std::string CKeyStorageService::GetTerminalKeyPath() {
  char buffer[MAX_PATH];
  if (GetModuleFileNameA(NULL, buffer, MAX_PATH) == 0)
    return "";
  std::string path(buffer);
  std::string::size_type pos = path.find_last_of("\\/");
  return path.substr(0, pos) + "\\" + TerminalKeyFileName;
}

bool CKeyStorageService::SaveTerminalPublicKey(const std::string &b64Key) {
  if (b64Key.empty())
    return false;

  std::string terminalKeyPath = GetTerminalKeyPath();
  if (terminalKeyPath.empty())
    return false;

  std::ofstream outfile(terminalKeyPath);
  if (!outfile.is_open())
    return false;
  outfile << b64Key;
  outfile.close();

  return SetTerminalPublicKey(b64Key);
}

bool CKeyStorageService::LoadTerminalPublicKey() {
  std::string terminalKeyPath = GetTerminalKeyPath();
  if (terminalKeyPath.empty())
    return false;

  std::ifstream infile(terminalKeyPath);
  if (!infile.is_open())
    return false;
  std::string b64Key;
  infile >> b64Key;
  infile.close();

  if (b64Key.empty())
    return false;

  // Strip UTF-8 BOM if present (EF BB BF)
  if (b64Key.size() >= 3 && (unsigned char)b64Key[0] == 0xEF &&
      (unsigned char)b64Key[1] == 0xBB && (unsigned char)b64Key[2] == 0xBF) {
    b64Key.erase(0, 3);
  }

  return SetTerminalPublicKey(b64Key);
}

// --------------------------------------------------------------------------------------
// EXPORT / IMPORT HELPERS
// --------------------------------------------------------------------------------------

std::vector<unsigned char>
CKeyStorageService::ExportPublicKeyToX509(NCRYPT_KEY_HANDLE hKey) {
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

BCRYPT_KEY_HANDLE CKeyStorageService::ImportPublicKeyFromX509(
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
    char msg[128];
    sprintf_s(msg,
              "Failed to import Terminal Key via CryptImportPublicKeyInfoEx2. "
              "Error: 0x%X",
              err);
    CFileLogService::Log("KeyStorage", msg);
    return NULL;
  }

  return hKey;
}

// --------------------------------------------------------------------------------------
// UTILS
// --------------------------------------------------------------------------------------

std::string
CKeyStorageService::Base64Encode(const std::vector<unsigned char> &data) {
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
CKeyStorageService::Base64Decode(const std::string &data) {
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

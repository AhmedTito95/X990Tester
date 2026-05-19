#pragma once
#include "CryptoService.h"
#include "KeyStorageService.h"
#include "Models.h"
#include "TcpClient.h"
#include <string>


class CTransactionService {
public:
  // Performs the INIT transaction
  // 1. Helper to generate/export PC Public Key
  // 2. Sends INIT request
  // 3. Updates KeyStorage with Terminal Public Key on success
  static PosResponse Init(CTcpClient &client, CKeyStorageService &keys,
                          const CString &ip, int port);

  // Performs the SALE transaction
  // 1. Encrypts request
  // 2. Sends
  // 3. Decrypts response
  static PosResponse Sale(CTcpClient &client, CKeyStorageService &keys,
                          const CString &ip, int port, int amount,
                          int currency, const std::string &receipt, int print,
                          int cashierId);

  // Performs the REFUND transaction
  static PosResponse Refund(CTcpClient &client, CKeyStorageService &keys,
                            const CString &ip, int port, int amount,
                            int currency, const std::string &receipt,
                            int seqNumber, const std::string &authCode,
                            const std::string &orgDate, int cashierId);
};

#pragma once
#include "Crypto.h"
#include "Models.h"
#include "TcpClient.h"
#include <string>


class CTransactionService {
public:
  // Performs the INIT transaction
  // 1. Helper to generate/export PC Public Key
  // 2. Sends INIT request
  // 3. Updates Crypto with Terminal Public Key on success
  static PosResponse Init(CTcpClient &client, CCrypto &crypto,
                          const CString &ip, int port);

  // Performs the SALE transaction
  // 1. Encrypts request
  // 2. Sends
  // 3. Decrypts response
  static PosResponse Sale(CTcpClient &client, CCrypto &crypto,
                          const CString &ip, int port, int amount,
                          int currency, const std::string &receipt, int print,
                          int cashierId);

  // Performs the REFUND transaction
  static PosResponse Refund(CTcpClient &client, CCrypto &crypto,
                            const CString &ip, int port, int amount,
                            int currency, const std::string &receipt,
                            int seqNumber, const std::string &authCode,
                            const std::string &orgDate, int cashierId);
};

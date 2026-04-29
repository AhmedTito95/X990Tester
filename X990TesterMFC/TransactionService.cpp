#include "pch.h"
#include "TransactionService.h"
#include "FileLogService.h"


using namespace std;

PosResponse CTransactionService::Init(CTcpClient &client, CCrypto &crypto,
                                      const std::string &ip, int port) {
  PosResponse errResp;
  errResp.ResponseCode = -1;

  // Ensure Crypto is ready (keys generated)
  // Note: In C#, AppSession setup keys. Here we assume Initialize() was called
  // or we call it. If we call it here, we regenerate keys every INIT.
  if (!crypto.Initialize()) {
    errResp.ErrorMessage = "Crypto Initialization Failed";
    return errResp;
  }

  InitRequest req;
  req.RsaPubKey = crypto.GetPcPublicKey();

  // Serialize
  string json = req.ToJson();

  // Send
  string responseJson = client.SendAndReceive(ip, port, json);
  CFileLogService::Log("InitRequest", json);
  CFileLogService::Log("InitResponse", responseJson);

  if (responseJson.empty()) {
    errResp.ErrorMessage = "No Response from Terminal";
    return errResp;
  }

  PosResponse resp = PosResponse::FromJson(responseJson);

  // If success, store the terminal key
  if (resp.ResponseCode == 0 && !resp.TerminalRsaPubKey.empty()) {
    if (!crypto.SetTerminalPublicKey(resp.TerminalRsaPubKey)) {
      resp.ResponseCode = -1;
      resp.ErrorMessage = "Failed to store Terminal Public Key";
    }
  }

  return resp;
}

PosResponse CTransactionService::Sale(CTcpClient &client, CCrypto &crypto,
                                      const std::string &ip, int port,
                                      int amount, int currency,
                                      const std::string &receipt, int print,
                                      int cashierId) {
  PosResponse errResp;
  errResp.ResponseCode = -1;

  SaleRequest req;
  req.Amount = to_string(amount);
  req.Currency = currency;
  req.ReceiptNumber = receipt;
  req.Print = print;
  req.CashierId = cashierId;

  string plainJson = req.ToJson();
  string encKey, encData;

  // Encrypt
  if (!crypto.Encrypt(plainJson, encKey, encData)) {
    errResp.ErrorMessage = "Encryption Failed";
    return errResp;
  }

  EncryptedPacket packet;
  packet.EncryptedAesKey = encKey;
  packet.EncryptedData = encData;

  string packetJson = packet.ToJson();

  // Send
  string responseJson = client.SendAndReceive(ip, port, packetJson);
  CFileLogService::Log("SaleRequest (Encrypted)", packetJson);
  CFileLogService::Log("SaleResponse (Encrypted)", responseJson);

  if (responseJson.empty()) {
    errResp.ErrorMessage = "No Response from Terminal";
    return errResp;
  }
 
  EncryptedPacket respPacket = EncryptedPacket::FromJson(responseJson);
  string clearJson =
      crypto.Decrypt(respPacket.EncryptedAesKey, respPacket.EncryptedData);

  CFileLogService::Log("SaleResponse (Decrypted)", clearJson);

  if (clearJson.empty()) {
    errResp.ErrorMessage = "Decryption Failed (Check Keys)";
    return errResp;
  }

  return PosResponse::FromJson(clearJson);
}

PosResponse CTransactionService::Refund(
    CTcpClient &client, CCrypto &crypto, const std::string &ip, int port,
    int amount, int currency, const std::string &receipt, int seqNumber,
    const std::string &authCode, const std::string &orgDate, int cashierId) {
  PosResponse errResp;
  errResp.ResponseCode = -1;

  RefundRequest req;
  req.Amount = to_string(amount);
  req.Currency = currency;
  req.ReceiptNumber = receipt;
  req.OrgSequenceNumber = seqNumber;
  req.OrgAuthCode = authCode;
  req.OrgDate = orgDate;
  req.CashierId = cashierId;

  string plainJson = req.ToJson();
  string encKey, encData;

  // Encrypt
  if (!crypto.Encrypt(plainJson, encKey, encData)) {
    errResp.ErrorMessage = "Encryption Failed";
    return errResp;
  }

  EncryptedPacket packet;
  packet.EncryptedAesKey = encKey;
  packet.EncryptedData = encData;

  string packetJson = packet.ToJson();

  // Send
  string responseJson = client.SendAndReceive(ip, port, packetJson);
  CFileLogService::Log("RefundRequest (Encrypted)", packetJson);
  CFileLogService::Log("RefundResponse (Encrypted)", responseJson);

  if (responseJson.empty()) {
    errResp.ErrorMessage = "No Response from Terminal";
    return errResp;
  }

  // Decrypt Response
  EncryptedPacket respPacket = EncryptedPacket::FromJson(responseJson);
  string clearJson =
      crypto.Decrypt(respPacket.EncryptedAesKey, respPacket.EncryptedData);

  CFileLogService::Log("RefundResponse (Decrypted)", clearJson);

  if (clearJson.empty()) {
    errResp.ErrorMessage = "Decryption Failed (Check Keys)";
    return errResp;
  }

  return PosResponse::FromJson(clearJson);
}

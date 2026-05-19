#include "pch.h"
#include "TransactionService.h"
#include "FileLogService.h"


using namespace std;

PosResponse CTransactionService::Init(CTcpClient &client,
                                      CKeyStorageService &keys,
                                      const CString &ip, int port) {
  PosResponse errResp;
  errResp.ResponseCode = -1;

  // Ensure keys are ready
  if (!keys.LoadOrCreatePcKey()) {
    errResp.ErrorMessage = "Key Storage Initialization Failed";
    return errResp;
  }

  InitRequest req;
  req.RsaPubKey = keys.GetPcPublicKeyBase64();

  // Serialize
  string json = req.ToJson();

  // Send
  CString responseJsonCString = client.SendAndReceive(ip, port, CString(json.c_str()));
  CStringA responseJsonA(responseJsonCString);
  string responseJson(responseJsonA.GetString());
  CFileLogService::Log("InitRequest", json);
  CFileLogService::Log("InitResponse", responseJson);

  if (responseJson.empty()) {
    errResp.ErrorMessage = "No Response from Terminal";
    return errResp;
  }

  PosResponse resp = PosResponse::FromJson(responseJson);

  // If success, store the terminal key
  if (resp.ResponseCode == 0 && !resp.TerminalRsaPubKey.empty()) {
    if (!keys.SetTerminalPublicKey(resp.TerminalRsaPubKey)) {
      resp.ResponseCode = -1;
      resp.ErrorMessage = "Failed to store Terminal Public Key";
    }
  }

  return resp;
}

PosResponse CTransactionService::Sale(CTcpClient &client,
                                      CKeyStorageService &keys,
                                      const CString &ip, int port,
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
  if (!CCryptoService::Encrypt(keys, plainJson, encKey, encData)) {
    errResp.ErrorMessage = "Encryption Failed";
    return errResp;
  }

  EncryptedPacket packet;
  packet.EncryptedAesKey = encKey;
  packet.EncryptedData = encData;

  string packetJson = packet.ToJson();

  // Send
  CString responseJsonCString = client.SendAndReceive(ip, port, CString(packetJson.c_str()));
  CStringA responseJsonA(responseJsonCString);
  string responseJson(responseJsonA.GetString());
  CFileLogService::Log("SaleRequest (Encrypted)", packetJson);
  CFileLogService::Log("SaleResponse (Encrypted)", responseJson);

  if (responseJson.empty()) {
    errResp.ErrorMessage = "No Response from Terminal";
    return errResp;
  }
 
  EncryptedPacket respPacket = EncryptedPacket::FromJson(responseJson);
  string clearJson =
      CCryptoService::Decrypt(keys, respPacket.EncryptedAesKey, respPacket.EncryptedData);

  CFileLogService::Log("SaleResponse (Decrypted)", clearJson);

  if (clearJson.empty()) {
    errResp.ErrorMessage = "Decryption Failed (Check Keys)";
    return errResp;
  }

  return PosResponse::FromJson(clearJson);
}

PosResponse CTransactionService::Refund(
    CTcpClient &client, CKeyStorageService &keys, const CString &ip, int port,
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
  if (!CCryptoService::Encrypt(keys, plainJson, encKey, encData)) {
    errResp.ErrorMessage = "Encryption Failed";
    return errResp;
  }

  EncryptedPacket packet;
  packet.EncryptedAesKey = encKey;
  packet.EncryptedData = encData;

  string packetJson = packet.ToJson();

  // Send
  CString responseJsonCString = client.SendAndReceive(ip, port, CString(packetJson.c_str()));
  CStringA responseJsonA(responseJsonCString);
  string responseJson(responseJsonA.GetString());
  CFileLogService::Log("RefundRequest (Encrypted)", packetJson);
  CFileLogService::Log("RefundResponse (Encrypted)", responseJson);

  if (responseJson.empty()) {
    errResp.ErrorMessage = "No Response from Terminal";
    return errResp;
  }

  // Decrypt Response
  EncryptedPacket respPacket = EncryptedPacket::FromJson(responseJson);
  string clearJson =
      CCryptoService::Decrypt(keys, respPacket.EncryptedAesKey, respPacket.EncryptedData);

  CFileLogService::Log("RefundResponse (Decrypted)", clearJson);

  if (clearJson.empty()) {
    errResp.ErrorMessage = "Decryption Failed (Check Keys)";
    return errResp;
  }

  return PosResponse::FromJson(clearJson);
}


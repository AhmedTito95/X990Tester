#pragma once
#include <string>
#include <vector>

// Using std::string to match the networking layer (typically C++ socket
// libraries use std::string) we can convert to CString for UI when needed.
using namespace std;

// Helper function declarations (implemented in cpp)
namespace JsonHelper {
string EscapeString(const string &input);
string UnescapeString(const string &input);
string ExtractString(const string &json, const string &key);
int ExtractInt(const string &json, const string &key);
} // namespace JsonHelper

struct EncryptedPacket {
  string EncryptedAesKey; // "k"
  string EncryptedData;   // "d"

  string ToJson() const;
  static EncryptedPacket FromJson(const string &json);
};

struct BaseRequest {
  int Lang = 0; // 0: English, 1: Arabic
  string Type;

  virtual string ToJson() const = 0;
  virtual ~BaseRequest() {}

protected:
  string BaseJsonFields() const;
};

struct InitRequest : public BaseRequest {
  string RsaPubKey;

  InitRequest();
  string ToJson() const override;
};

struct SaleRequest : public BaseRequest {
  string Amount; // No decimals
  int Currency;
  string ReceiptNumber;
  int CashierId;
  int Print = 1;

  SaleRequest();
  string ToJson() const override;
};

struct RefundRequest : public BaseRequest {
  string Amount;
  int Currency;
  string ReceiptNumber;
  int CashierId;
  int OrgSequenceNumber;
  string OrgAuthCode;
  string OrgDate;

  RefundRequest();
  string ToJson() const override;
};

struct PosResponse {
  int ResponseCode;         // "resp_code"
  string ErrorMessage;      // "error_msg"
  string TerminalRsaPubKey; // "rsaPubKey"
  string AuthCode;          // "auth_code"

  // Additional fields can be added as needed
  string TransactionDate; // "datim"
  int SequenceNumber;     // "seq"

  static PosResponse FromJson(const string &json);
};

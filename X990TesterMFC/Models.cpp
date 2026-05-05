#include "pch.h"
#include "Models.h"
#include <sstream>

// Basic Json Helper Implementation
namespace JsonHelper {
string EscapeString(const string &input) {
  string output;
  for (char c : input) {
    if (c == '"')
      output += "\\\"";
    else if (c == '\\')
      output += "\\\\";
    else
      output += c;
  }
  return output;
}

string UnescapeString(const string &input) {
  string output;
  for (size_t i = 0; i < input.length(); ++i) {
    if (input[i] == '\\' && i + 1 < input.length()) {
      char next = input[i + 1];
      if (next == '"') {
        output += '"';
        i++;
      } else if (next == '\\') {
        output += '\\';
        i++;
      } else if (next == 'n') {
        output += '\n';
        i++;
      } else if (next == 'r') {
        output += '\r';
        i++;
      } else if (next == 't') {
        output += '\t';
        i++;
      } else if (next == 'u' && i + 5 < input.length()) {
        // \uXXXX — parse 4 hex digits into a Unicode codepoint
        char hex[5] = { input[i+2], input[i+3], input[i+4], input[i+5], '\0' };
        unsigned int codepoint = strtoul(hex, nullptr, 16);
        // Encode as UTF-8 (covers BMP chars, including all base64 special chars)
        if (codepoint < 0x80) {
          output += (char)codepoint;
        } else if (codepoint < 0x800) {
          output += (char)(0xC0 | (codepoint >> 6));
          output += (char)(0x80 | (codepoint & 0x3F));
        } else {
          output += (char)(0xE0 | (codepoint >> 12));
          output += (char)(0x80 | ((codepoint >> 6) & 0x3F));
          output += (char)(0x80 | (codepoint & 0x3F));
        }
        i += 5;
      } else {
        output += next; // fallback: pass through
        i++;
      }
    } else {
      output += input[i];
    }
  }
  return output;
}


// Very naive JSON extractor. Assumes simple non-nested or simple nested
// structure. Finds "key":"value" or "key":123
string ExtractString(const string &json, const string &key) {
  string keyPattern = "\"" + key + "\":";
  size_t pos = json.find(keyPattern);
  if (pos == string::npos)
    return "";

  pos += keyPattern.length();

  // Skip whitespace
  while (pos < json.length() && isspace(json[pos]))
    pos++;

  if (pos >= json.length())
    return "";

  if (json[pos] == '"') {
    // String value
    size_t start = pos + 1;
    size_t end = start;
    while (end < json.length()) {
      if (json[end] == '"' && json[end - 1] != '\\')
        break;
      end++;
    }
    return UnescapeString(json.substr(start, end - start));
  }
  return "";
}

int ExtractInt(const string &json, const string &key) {
  string keyPattern = "\"" + key + "\":";
  size_t pos = json.find(keyPattern);
  if (pos == string::npos)
    return 0;

  pos += keyPattern.length();
  // Skip whitespace
  while (pos < json.length() && isspace(json[pos]))
    pos++;

  if (pos >= json.length())
    return 0;

  // Parse integer up to comma or closing brace
  size_t end = pos;
  while (end < json.length() && (isdigit(json[end]) || json[end] == '-'))
    end++;

  string val = json.substr(pos, end - pos);
  return atoi(val.c_str());
}
} // namespace JsonHelper

// EncryptedPacket
string EncryptedPacket::ToJson() const {
  // {"k":"...", "d":"..."}
  return "{\"k\":\"" + JsonHelper::EscapeString(EncryptedAesKey) +
         "\", \"d\":\"" + JsonHelper::EscapeString(EncryptedData) + "\"}";
}

EncryptedPacket EncryptedPacket::FromJson(const string &json) {
  EncryptedPacket p;
  p.EncryptedAesKey = JsonHelper::ExtractString(json, "k");
  p.EncryptedData = JsonHelper::ExtractString(json, "d");
  return p;
}

// BaseRequest
string BaseRequest::BaseJsonFields() const {
  return "\"lang\":" + to_string(Lang) + ", \"type\":\"" +
         JsonHelper::EscapeString(Type) + "\"";
}

// InitRequest
InitRequest::InitRequest() { Type = "INIT"; }

string InitRequest::ToJson() const {
  return "{" + BaseJsonFields() + ", \"rsaPubKey\":\"" +
         JsonHelper::EscapeString(RsaPubKey) + "\"}";
}

// SaleRequest
SaleRequest::SaleRequest() { Type = "SALE"; }

string SaleRequest::ToJson() const {
  return "{" + BaseJsonFields() + ", \"amt\":\"" +
         JsonHelper::EscapeString(Amount) + "\"" +
         ", \"cur\":" + to_string(Currency) + ", \"rcpt\":\"" +
         JsonHelper::EscapeString(ReceiptNumber) + "\"" +
         ", \"cshr\":" + to_string(CashierId) +
         ", \"print\":" + to_string(Print) + "}";
}

// RefundRequest
RefundRequest::RefundRequest() { Type = "RETURN"; }

string RefundRequest::ToJson() const 
{
  return "{" + BaseJsonFields() + ", \"amt\":\"" +
         JsonHelper::EscapeString(Amount) + "\"" +
         ", \"cur\":" + to_string(Currency) + ", \"rcpt\":\"" +
         JsonHelper::EscapeString(ReceiptNumber) + "\"" +
         ", \"cshr\":" + to_string(CashierId) +
         ", \"origSeq\":" + to_string(OrgSequenceNumber) + ", \"authCode\":\"" +
         JsonHelper::EscapeString(OrgAuthCode) + "\"" + ", \"datim\":\"" +
         JsonHelper::EscapeString(OrgDate) + "\"" + "}";
}

// PosResponse
PosResponse PosResponse::FromJson(const string &json) 
{
  PosResponse r;
  r.ResponseCode = JsonHelper::ExtractInt(json, "resp_code");
  r.ErrorMessage = JsonHelper::ExtractString(json, "error_msg");
  r.TerminalRsaPubKey = JsonHelper::ExtractString(json, "rsaPubKey");
  r.AuthCode = JsonHelper::ExtractString(json, "auth_code");
  r.TransactionDate = JsonHelper::ExtractString(json, "datim");
  r.SequenceNumber = JsonHelper::ExtractInt(json, "seq");
  return r;
}

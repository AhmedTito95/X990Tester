#include "pch.h"
#include "TcpClient.h"
#include <iomanip>
#include <iostream>
#include <sstream>
#include <ws2tcpip.h>


using namespace std;

CTcpClient::CTcpClient() {
  WSADATA wsa;
  WSAStartup(MAKEWORD(2, 2), &wsa);
}

CTcpClient::~CTcpClient() { WSACleanup(); }

std::string CTcpClient::WrapFrame(const std::string &json) {
  // ~PCNC~{len:D4}~{json}
  std::stringstream ss;
  ss << "~PCNC~" << setfill('0') << setw(4) << json.length() << "~" << json;
  return ss.str();
}

std::string CTcpClient::UnwrapFrame(const std::string &framed) {
  size_t first = framed.find('{');
  size_t last = framed.rfind('}');
  if (first == string::npos || last == string::npos || first >= last)
    return "";
  return framed.substr(first, last - first + 1);
}

SOCKET CTcpClient::Connect(const std::string &ip, int port) {
  SOCKET s = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
  if (s == INVALID_SOCKET)
    return INVALID_SOCKET;

  sockaddr_in addr;
  addr.sin_family = AF_INET;
  addr.sin_port = htons(port);
  inet_pton(AF_INET, ip.c_str(), &addr.sin_addr);

  // Connect with timeout (blocking connect is default)
  if (connect(s, (SOCKADDR *)&addr, sizeof(addr)) == SOCKET_ERROR) {
    closesocket(s);
    return INVALID_SOCKET;
  }
  return s;
}

void CTcpClient::Close(SOCKET sock) {
  if (sock != INVALID_SOCKET) {
    shutdown(sock, SD_BOTH);
    closesocket(sock);
  }
}

bool CTcpClient::TestConnection(const std::string &ip, int port) {
  SOCKET s = Connect(ip, port);
  if (s == INVALID_SOCKET)
    return false;
  Close(s);
  return true;
}

std::string CTcpClient::SendAndReceive(const std::string &ip, int port,
                                       const std::string &json) {
  SOCKET s = Connect(ip, port);
  if (s == INVALID_SOCKET)
    return "";

  std::string framed = WrapFrame(json);

  // Send
  if (send(s, framed.c_str(), (int)framed.length(), 0) == SOCKET_ERROR) {
    Close(s);
    return "";
  }

  // Read
  // We emulate "read what's available" with a timeout logic.
  // 1. Set Receive Timeout
  DWORD timeout = 40*1000; 
  setsockopt(s, SOL_SOCKET, SO_RCVTIMEO, (const char *)&timeout,
             sizeof(timeout));

  std::string buffer;
  char temp[4096];

  // Read loop
  // Since we don't strictly parse length header in C# unwrapper, we just read
  // until we think we're done or timeout? C# reads "while DataAvailable". We'll
  // read at least once. Then check if more is coming.

  while (true) {
    int r = recv(s, temp, sizeof(temp) - 1, 0);
    if (r > 0) {
      temp[r] = '\0';
      buffer += temp;

      // Check content length if possible to quit early
      // If buffer has } at the end, likely done (JSON).
      if (buffer.length() > 0 && buffer.back() == '}') {
        // Peek? Or just wait 100ms for more?
        // Let's assume one response packet for now, or check buffer length vs
        // frame header? Frame header: ~PCNC~LLLL~ (11 chars)
        if (buffer.length() >= 11) {
          // Try to parse length
          if (buffer.substr(0, 6) == "~PCNC~") {
            try {
              int len = stoi(buffer.substr(6, 4));
              // Total expected: 11 + len
              if (buffer.length() >= 11 + len) {
                break; // Done
              }
            } catch (...) {
            }
          }
        }
      }
    } else if (r == 0) {
      break; // Closed
    } else {
      // Error or Timeout
      break;
    }
  }

  Close(s);
  return UnwrapFrame(buffer);
}

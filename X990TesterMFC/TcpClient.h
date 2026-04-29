#pragma once
#include <string>
#include <vector>
#include <winsock2.h>


// Link with ws2_32.lib
#pragma comment(lib, "ws2_32.lib")

class CTcpClient {
public:
  CTcpClient();
  ~CTcpClient();

  // Sends a JSON request and waits for a response (Short-lived connection)
  // 1. Connects
  // 2. Wraps JSON in frame
  // 3. Sends
  // 4. Reads response
  // 5. Unwraps JSON
  // 6. Disconnects
  std::string SendAndReceive(const std::string &ip, int port,
                             const std::string &json);

  // Tests connection ability
  bool TestConnection(const std::string &ip, int port);

private:
  std::string WrapFrame(const std::string &json);
  std::string UnwrapFrame(const std::string &framed);

  // Internal helpers
  SOCKET Connect(const std::string &ip, int port);
  void Close(SOCKET sock);
};

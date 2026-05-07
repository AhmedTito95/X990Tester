#pragma once
#include <afx.h>
#include <vector>
#include <string>

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
  CString SendAndReceive(const CString &ip, int port,
                         const CString &json);

  // Tests connection ability
  bool TestConnection(const CString &ip, int port);

private:
  std::string WrapFrame(const std::string &json);
  std::string UnwrapFrame(const std::string &framed);
};

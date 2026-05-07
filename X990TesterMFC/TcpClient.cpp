#include "pch.h"
#include "TcpClient.h"
#include <iomanip>
#include <iostream>
#include <sstream>
#include <boost/asio.hpp>
#include <array>
#include <atlbase.h>

using namespace std;
using boost::asio::ip::tcp;

CTcpClient::CTcpClient() {
}

CTcpClient::~CTcpClient() {
}

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

bool CTcpClient::TestConnection(const CString &ip, int port) {
  try {
    boost::asio::io_context io_context;
    tcp::resolver resolver(io_context);
    CStringA ipA(ip); // Convert IP string to narrow for boost
    tcp::resolver::results_type endpoints = resolver.resolve(std::string(ipA), std::to_string(port));

    tcp::socket socket(io_context);
    boost::asio::connect(socket, endpoints);
    socket.close();
    return true;
  } catch (std::exception&) {
    return false;
  }
}

CString CTcpClient::SendAndReceive(const CString &ip, int port,
                                   const CString &json) {
  try {
    boost::asio::io_context io_context;
    tcp::resolver resolver(io_context);
    CStringA ipA(ip); // Convert IP string to narrow for boost
    tcp::resolver::results_type endpoints = resolver.resolve(std::string(ipA), std::to_string(port));

    tcp::socket socket(io_context);
    boost::asio::connect(socket, endpoints);

    CStringA jsonA(json);
    std::string stdJson(jsonA.GetString(), jsonA.GetLength());
    std::string framed = WrapFrame(stdJson);
    boost::asio::write(socket, boost::asio::buffer(framed));

    // Set receive timeout
    DWORD timeout = 40 * 1000;
    setsockopt(socket.native_handle(), SOL_SOCKET, SO_RCVTIMEO, (const char *)&timeout,
               sizeof(timeout));

    std::string buffer;
    std::array<char, 4096> temp;
    boost::system::error_code error;

    // Use std::string internally for parsing raw network bytes
    while (true) {
      size_t r = socket.read_some(boost::asio::buffer(temp), error);
      if (error == boost::asio::error::eof)
        break; // Connection closed cleanly by peer.
      else if (error)
        break; // Some other error.

      if (r > 0) {
        buffer.append(temp.data(), r);

        // Check content length if possible to quit early
        if (buffer.length() > 0 && buffer.back() == '}') {
          if (buffer.length() >= 11) {
            if (buffer.substr(0, 6) == "~PCNC~") {
              try {
                int len = stoi(buffer.substr(6, 4));
                if (buffer.length() >= 11 + len) {
                  break; // Done
                }
              } catch (...) {
              }
            }
          }
        }
      }
    }

    boost::system::error_code ignored_ec;
    socket.shutdown(boost::asio::ip::tcp::socket::shutdown_both, ignored_ec);
    socket.close();

    // Convert network byte buffer back to CString
    std::string unwrapped = UnwrapFrame(buffer);
    return CString(unwrapped.c_str());
  } catch (std::exception&) {
    return _T("");
  }
}

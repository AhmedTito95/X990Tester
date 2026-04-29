#include "pch.h"
#include "FileLogService.h"
#include <fstream>
#include <iomanip>
#include <mutex>
#include <windows.h>


using namespace std;

// Mutex for thread safety
static std::mutex g_logMutex;

void CFileLogService::Log(const std::string &stage,
                          const std::string &message) {
  try {
    std::lock_guard<std::mutex> lock(g_logMutex);

    std::string logDir = GetLogDirectory();

    // Ensure directory exists
    if (GetFileAttributesA(logDir.c_str()) == INVALID_FILE_ATTRIBUTES) {
      CreateDirectoryA(logDir.c_str(), NULL);
    }

    std::string dateStr = GetCurrentDate(); // yyyyMMdd
    std::string filePath = logDir + "\\x990_" + dateStr + ".log";

    std::ofstream outfile;
    // Append mode
    outfile.open(filePath, std::ios_base::app);

    if (outfile.is_open()) {
      outfile << "--------------------------------------------------" << endl;
      outfile << "Time      : " << GetCurrentTimestamp() << endl;
      outfile << "Stage     : " << stage << endl;
      outfile << "Message   :" << endl;
      outfile << message << endl;
      outfile << endl;
      outfile.close();
    }
  } catch (...) {
    // Suppress errors
  }
}

std::string CFileLogService::GetLogDirectory() {
  char buffer[MAX_PATH];
  if (GetModuleFileNameA(NULL, buffer, MAX_PATH) == 0) {
    return "Logs";
  }
  std::string path(buffer);
  std::string::size_type pos = path.find_last_of("\\/");
  if (pos == std::string::npos)
    return "Logs";

  std::string dir = path.substr(0, pos);
  return dir + "\\Logs";
}

std::string CFileLogService::GetCurrentTimestamp() {
  SYSTEMTIME st;
  GetLocalTime(&st);
  char buf[64];
  sprintf_s(buf, "%04d-%02d-%02d %02d:%02d:%02d.%03d", st.wYear, st.wMonth,
            st.wDay, st.wHour, st.wMinute, st.wSecond, st.wMilliseconds);
  return std::string(buf);
}

std::string CFileLogService::GetCurrentDate() {
  SYSTEMTIME st;
  GetLocalTime(&st);
  char buf[64];
  sprintf_s(buf, "%04d%02d%02d", st.wYear, st.wMonth, st.wDay);
  return std::string(buf);
}

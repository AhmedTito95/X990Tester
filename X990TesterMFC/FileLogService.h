#pragma once
#include <string>

class CFileLogService {
public:
  // Log a message with a specific stage tag to a daily log file
  static void Log(const std::string &stage, const std::string &message);

private:
  static std::string GetLogDirectory();
  static std::string GetCurrentTimestamp();
  static std::string GetCurrentDate();
};

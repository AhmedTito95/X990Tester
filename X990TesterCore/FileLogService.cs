using System;
using System.IO;
using System.Text;

namespace X990TesterCore
{
    public static class FileLoggingService
    {
        private static readonly object _lock = new object();

        private static readonly string _logDir =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

        public static void Log(string stage, string message)
        {
            try
            {
                if (!Directory.Exists(_logDir))
                    Directory.CreateDirectory(_logDir);

                string filePath = Path.Combine(
                    _logDir,
                    $"x990_{DateTime.Now:yyyyMMdd}.log");

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("--------------------------------------------------");
                sb.AppendLine($"Time      : {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
                sb.AppendLine($"Stage     : {stage}");
                sb.AppendLine("Message   :");
                sb.AppendLine(message);
                sb.AppendLine();

                lock (_lock)
                {
                    File.AppendAllText(filePath, sb.ToString(), Encoding.UTF8);
                }
            }
            catch
            {
            }
        }
    }
}

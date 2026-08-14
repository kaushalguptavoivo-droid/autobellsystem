using System;
using System.IO;

namespace AutoBellSystem.Services
{
    /// <summary>
    /// Writes crashes and important events to a plain text log file so
    /// problems can be diagnosed without a debugger - just open the file
    /// and read/share it.
    /// </summary>
    public static class Logger
    {
        public static readonly string LogFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AutoBellSystem", "logs");

        public static readonly string LogFile = Path.Combine(LogFolder, "crash.log");

        public static void LogError(string context, Exception ex)
        {
            try
            {
                Directory.CreateDirectory(LogFolder);
                var entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR in {context}:{Environment.NewLine}{ex}{Environment.NewLine}{new string('-', 60)}{Environment.NewLine}";
                File.AppendAllText(LogFile, entry);
            }
            catch
            {
                // If we can't even write the log, there's nothing more we can do here.
            }
        }

        public static void LogInfo(string message)
        {
            try
            {
                Directory.CreateDirectory(LogFolder);
                var entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] INFO: {message}{Environment.NewLine}";
                File.AppendAllText(LogFile, entry);
            }
            catch
            {
                // ignore
            }
        }
    }
}

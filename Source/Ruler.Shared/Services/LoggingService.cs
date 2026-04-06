using System;
using System.Diagnostics;

namespace Ruler.Shared
{
    /// <summary>
    /// Simple implementation that outputs to the Debug console with class context.
    /// </summary>
    public class LoggingService<T> : ILoggingService<T>
    {
        private readonly string _className;

        public LoggingService()
        {
            _className = typeof(T).Name;
        }

        public void LogInfo(string message)
        {
            WriteLog("INFO", message);
        }

        public void LogWarning(string message)
        {
            WriteLog("WARN", message);
        }

        public void LogError(string message, Exception ex = null)
        {
            WriteLog("ERROR", message);
            if (ex != null)
            {
                Debug.WriteLine($"[Details]: {ex}");
            }
        }

        private void WriteLog(string level, string message)
        {
            Debug.WriteLine($"[{level}][{_className}] {DateTime.Now:HH:mm:ss.fff}: {message}");
        }
    }
}

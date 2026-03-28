using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ruler.Shared.Interfaces;

namespace Ruler.Shared.Services
{
    /// <summary>
    /// Simple implementation that outputs to the Debug console with class context.
    /// </summary>
    public class DebugLoggingService<T> : ILoggingService<T>
    {
        private readonly string _className;

        public DebugLoggingService()
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

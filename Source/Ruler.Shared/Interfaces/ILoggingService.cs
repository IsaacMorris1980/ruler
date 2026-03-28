using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Interfaces
{
   /// <summary>
    /// A generic logging interface that tracks the source class.
    /// </summary>
    public interface ILoggingService<T>
    {
        void LogInfo(string message);
        void LogError(string message, Exception ex = null);
        void LogWarning(string message);
    }

}

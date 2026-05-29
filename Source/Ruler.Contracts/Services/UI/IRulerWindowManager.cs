using Ruler.Contracts.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Contracts.Services.UI
{
    public interface IRulerWindowManager
    {
        /// <summary>
        /// Instantiates a physical window shell, binds its DataContext view model,
        /// and returns the native window handle (HWND) for low-level tracking.
        /// </summary>
        IntPtr CreateWindow(IRulerInfo info, Action onClosed);

        /// <summary>
        /// Explicitly terminates the UI platform framework execution loop.
        /// </summary>
        void ShutdownApplication();
    }
}

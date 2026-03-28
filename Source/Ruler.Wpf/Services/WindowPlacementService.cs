using Ruler.Wpf.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

using static Ruler.Wpf.Common.NativeMethods;

namespace Ruler.Wpf.Services
{
    public class WindowPlacementService : IWindowPlacementService
    {
        public void EnsureVisibility(Window window)
        {
            bool isVisible = false;
            var windowRect = GetWindowRect(window);

            NativeMethods.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (IntPtr h, IntPtr hdc, ref NativeMethods.Rect monitorRect, IntPtr data) =>
            {
                if (windowRect.Left < monitorRect.Right && windowRect.Right > monitorRect.Left &&
                    windowRect.Top < monitorRect.Bottom && windowRect.Bottom > monitorRect.Top)
                {
                    isVisible = true;
                    return false;
                }
                return true;
            }, IntPtr.Zero);

            if (!isVisible)
            {
                window.Left = 100;
                window.Top = 100;
            }
        }

        public void SpanAllMonitors(Window window)
        {
            int minL = 0, minT = 0, maxR = 0, maxB = 0;

            NativeMethods.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (IntPtr h, IntPtr hdc, ref NativeMethods.Rect r, IntPtr d) =>
            {
                minL = Math.Min(minL, r.Left);
                minT = Math.Min(minT, r.Top);
                maxR = Math.Max(maxR, r.Right);
                maxB = Math.Max(maxB, r.Bottom);
                return true;
            }, IntPtr.Zero);

            window.Left = minL;
            window.Top = minT;
            window.Width = maxR - minL;
            window.Height = maxB - minT;
        }

        public MonitorPositionData GetMonitorRelativePosition(Window window)
        {
            var hwnd = new WindowInteropHelper(window).Handle;
            var hMonitor = NativeMethods.MonitorFromWindow(hwnd, NativeMethods.MONITOR_DEFAULTTONEAREST);
            var info = new NativeMethods.MONITORINFOEX { Size = Marshal.SizeOf(typeof(NativeMethods.MONITORINFOEX)) };

            if (NativeMethods.GetMonitorInfo(hMonitor, ref info))
            {
                return new MonitorPositionData
                {
                    DeviceId = info.DeviceName,
                    RelativeX = window.Left - info.Monitor.Left,
                    RelativeY = window.Top - info.Monitor.Top
                };
            }
            return default;
        }

        public Point GetVirtualPosition(MonitorPositionData savedData)
        {
            var screen = Screen.AllScreens.FirstOrDefault(s => s.DeviceName == savedData.DeviceId);
            if (screen != null)
                return new Point(screen.Bounds.Left + savedData.RelativeX, screen.Bounds.Top + savedData.RelativeY);

            return new Point(100, 100);
        }

        private NativeMethods.Rect GetWindowRect(Window w) => new NativeMethods.Rect
        {
            Left = (int)w.Left,
            Top = (int)w.Top,
            Right = (int)(w.Left + w.Width),
            Bottom = (int)(w.Top + w.Height)
        };
    }
}

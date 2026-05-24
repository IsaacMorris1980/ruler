using Ruler.Contracts.Interop;
using Ruler.Contracts.Services.OS;
using Ruler.Shared;

using System;
using System.Runtime.InteropServices;


namespace Ruler.Wpf.Services
{
    public class WindowPlacementService : IWindowPlacementService
    {
        /// <summary>
        /// Validates that an unmanaged window handle sits within readable monitor bounds.
        /// If it is completely off-screen, it corrects its position to safe fallback coordinates.
        /// </summary>
        public void EnsureVisibility(IntPtr window)
        {
            if (window == IntPtr.Zero) return;

            // 1. Fetch current window boundaries using your NativeMethods class
            if (!NativeMethods.GetWindowRect(window, out Ruler.Contracts.Interop.NativeStructures.RECT windowRect))
            {
                return;
            }

            bool isVisibleOnAnyMonitor = false;

            // 2. Enumerate system displays to check for intersecting boundaries
            NativeMethods.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, delegate (IntPtr hMonitor, IntPtr hdcMonitor, ref Ruler.Contracts.Interop.NativeStructures.RECT monitorRect, IntPtr dwData)
            {
                if (windowRect.Left < monitorRect.Right && windowRect.Right > monitorRect.Left &&
                    windowRect.Top < monitorRect.Bottom && windowRect.Bottom > monitorRect.Top)
                {
                    isVisibleOnAnyMonitor = true;
                    return false; // Intersection validated, stop checking other monitors
                }
                return true;
            }, IntPtr.Zero);

            // 3. If lost off-screen, snap its top-left coordinates back to a safe default (100, 100)
            if (!isVisibleOnAnyMonitor)
            {
                int safeLeft = 100;
                int safeTop = 100;
                uint flags = NativeConstants.SWP_NOSIZE | NativeConstants.SWP_NOZORDER | NativeConstants.SWP_NOACTIVATE;

                // 0, 0 are passed for width/height because SWP_NOSIZE explicitly forces Windows to ignore them
                NativeMethods.SetWindowPos(window, IntPtr.Zero, safeLeft, safeTop, 0, 0, flags);
                return; // Signals to orchestration that a recovery action was taken
            }

            return; // Window is safely viewable where it stands
        }

        /// <summary>
        /// Resizes and positions an unmanaged window boundary to completely span across all detected displays.
        /// </summary>
        public void SpanAllMonitors(IntPtr window)
        {
            if (window == IntPtr.Zero) return;

            int minLeft = 0;
            int minTop = 0;
            int maxRight = 0;
            int maxBottom = 0;

            // Enumerate displays to track extreme desktop frame edges
            NativeMethods.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, delegate (IntPtr hMonitor, IntPtr hdcMonitor, ref Ruler.Contracts.Interop.NativeStructures.RECT monitorRect, IntPtr dwData)
            {
                if (monitorRect.Left < minLeft) minLeft = monitorRect.Left;
                if (monitorRect.Top < minTop) minTop = monitorRect.Top;
                if (monitorRect.Right > maxRight) maxRight = monitorRect.Right;
                if (monitorRect.Bottom > maxBottom) maxBottom = monitorRect.Bottom;
                return true;
            }, IntPtr.Zero);

            int fullWidth = maxRight - minLeft;
            int fullHeight = maxBottom - minTop;
            uint flags = NativeConstants.SWP_NOZORDER | NativeConstants.SWP_NOACTIVATE;

            // This sets both position (minLeft, minTop) and dimensions (fullWidth, fullHeight) simultaneously
            NativeMethods.SetWindowPos(window, IntPtr.Zero, minLeft, minTop, fullWidth, fullHeight, flags);
        }

        /// <summary>
        /// Identifies the monitor display closest to the window handle and calculates 
        /// coordinates relative to that specific display's top-left corner.
        /// </summary>
        public Ruler.Contracts.Interop.NativeStructures.MonitorPositionData GetMonitorRelativePosition(IntPtr window)
        {
            if (window == IntPtr.Zero || !NativeMethods.GetWindowRect(window, out Ruler.Contracts.Interop.NativeStructures.RECT windowRect))
            {
                return default(Ruler.Contracts.Interop.NativeStructures.MonitorPositionData);
            }

            // Get closest monitor handle
            IntPtr hMonitor = NativeMethods.MonitorFromWindow(window, NativeConstants.MONITOR_DEFAULTTONEAREST);

            Ruler.Shared.NativeStructures.MONITORINFOEX info = new Ruler.Shared.NativeStructures.MONITORINFOEX();
            info.Size = Marshal.SizeOf(typeof(Ruler.Shared.NativeStructures.MONITORINFOEX));

            if (NativeMethods.GetMonitorInfo(hMonitor, ref info))
            {
                Ruler.Contracts.Interop.NativeStructures.MonitorPositionData data = new Ruler.Contracts.Interop.NativeStructures.MonitorPositionData();
                data.DeviceId = info.DeviceName; // Matches exact Windows identity key (e.g. "\\.\DISPLAY1")
                data.RelativeX = windowRect.Left - info.Monitor.Left;
                data.RelativeY = windowRect.Top - info.Monitor.Top;
                return data;
            }

            return default(Ruler.Contracts.Interop.NativeStructures.MonitorPositionData);
        }

        /// <summary>
        /// Translates a saved monitor identity and relative offset back into absolute virtual desktop screen coordinates.
        /// </summary>
        public Ruler.Contracts.Interop.NativeStructures.POINT GetVirtualPosition(Ruler.Contracts.Interop.NativeStructures.MonitorPositionData savedData)
        {
            bool monitorFound = false;
            int targetMonitorLeft = 0;
            int targetMonitorTop = 0;

            // Loop monitors to locate the exact active display hardware device match
            NativeMethods.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, delegate (IntPtr hMonitor, IntPtr hdcMonitor, ref Ruler.Contracts.Interop.NativeStructures.RECT monitorRect, IntPtr dwData)
            {
                Ruler.Shared.NativeStructures.MONITORINFOEX info = new Ruler.Shared.NativeStructures.MONITORINFOEX();
                info.Size = Marshal.SizeOf(typeof(Ruler.Shared.NativeStructures.MONITORINFOEX));

                if (NativeMethods.GetMonitorInfo(hMonitor, ref info))
                {
                    if (info.DeviceName == savedData.DeviceId)
                    {
                        targetMonitorLeft = info.Monitor.Left;
                        targetMonitorTop = info.Monitor.Top;
                        monitorFound = true;
                        return false; // Match discovered; abort verification loop
                    }
                }
                return true;
            }, IntPtr.Zero);

            Ruler.Contracts.Interop.NativeStructures.POINT targetCoordinates = new Ruler.Contracts.Interop.NativeStructures.POINT();

            if (monitorFound)
            {
                // Map local offset space onto concrete virtual screen positions
                targetCoordinates.x = (int)(targetMonitorLeft + savedData.RelativeX);
                targetCoordinates.y = (int)(targetMonitorTop + savedData.RelativeY);
            }
            else
            {
                // Unplugged layout fallback coordinates
                targetCoordinates.x = 100;
                targetCoordinates.y = 100;
            }

            return targetCoordinates;
        }
    }
}

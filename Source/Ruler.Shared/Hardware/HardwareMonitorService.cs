
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
namespace Ruler.Shared
{
    public class HardwareMonitorService : IHardwareMonitorService
    {
        /// <summary>
        /// Gets the effective DPI for the monitor where the window is located.
        /// Uses NativeMethods to bridge to Shcore.dll.
        /// </summary>
        public double GetEffectiveDpi(IntPtr windowHandle)
        {
            // 1. Get the Monitor Handle from the Window Handle
            IntPtr hMonitor = NativeMethods.MonitorFromWindow(
                windowHandle,
                NativeConstants.MONITOR_DEFAULTTONEAREST);
            if (hMonitor != IntPtr.Zero)
            {
                // 2. Get the DPI from the Monitor Handle
                int result = NativeMethods.GetDpiForMonitor(
                    hMonitor,
                    MonitorDpiType.EffectiveDpi,
                    out uint dpiX,
                    out uint dpiY);
                if (result == NativeConstants.S_OK)
                {
                    return (double)dpiX;
                }
            }
            return 96.0; // Standard fallback (100% scaling)
        }
        /// <summary>
        /// Generates a stable ID for the monitor to persist ruler positions.
        /// </summary>
        public string GetHardwareId(IntPtr windowHandle)
        {
            IntPtr hMonitor = NativeMethods.MonitorFromWindow(
                windowHandle,
                NativeConstants.MONITOR_DEFAULTTONEAREST);
            // We use the Monitor Info structure to get the Device Name (e.g., \\.\DISPLAY1)
            NativeStructures.MONITORINFOEX info = new NativeStructures.MONITORINFOEX();
            info.Size = Marshal.SizeOf(info);
            if (NativeMethods.GetMonitorInfo(hMonitor, ref info))
            {
                return info.DeviceName.TrimEnd('\0');
            }
            return "Unknown_Monitor";
        }
        public IntPtr GetMonitorHandle(IntPtr windowHandle)
        {
            return NativeMethods.MonitorFromWindow(
                windowHandle,
                NativeConstants.MONITOR_DEFAULTTONEAREST);
        }
        /// <summary>
        /// Returns all hardware profiles (monitors) currently recognized by the OS.
        /// </summary>
        public List<string> GetActiveHardwareProfiles()
        {
            var profiles = new List<string>();
            NativeMethods.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (hMonitor, hdc, lprc, lparam) =>
            {
                NativeStructures.MONITORINFOEX info = new NativeStructures.MONITORINFOEX();
                info.Size = Marshal.SizeOf(info);
                if (NativeMethods.GetMonitorInfo(hMonitor, ref info))
                {
                    profiles.Add(new string(info.DeviceName).TrimEnd('\0'));
                }
                return true;
            }, IntPtr.Zero);
            return profiles;
        }
    }
}


using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Ruler.Shared;
using Ruler.Contracts.Models;
using Ruler.Contracts.Services.OS;
namespace Ruler.Shared
{
    public class HardwareMonitorService : IHardwareMonitorService
    {
        public double GetEffectiveDpi(IntPtr windowHandle)
        {
            IntPtr hMonitor = NativeMethods.MonitorFromWindow(
                windowHandle,
                NativeConstants.MONITOR_DEFAULTTONEAREST);

            if (hMonitor != IntPtr.Zero)
            {
                // NativeEnums.MonitorDpiType.EffectiveDpi used here
                int result = NativeMethods.GetDpiForMonitor(
                    hMonitor,
                    MonitorDpiType.EffectiveDpi,
                    out uint dpiX,
                    out uint dpiY);

                if (result == (int)NativeConstants.S_OK)
                {
                    return (double)dpiX;
                }
            }
            return 96.0; // Standard 100% fallback
        }

        public string GetHardwareId(IntPtr windowHandle)
        {
            IntPtr hMonitor = NativeMethods.MonitorFromWindow(
                windowHandle,
                NativeConstants.MONITOR_DEFAULTTONEAREST);

            NativeStructures.MONITORINFOEX info = new NativeStructures.MONITORINFOEX();
            info.Size = Marshal.SizeOf(typeof(NativeStructures.MONITORINFOEX)); // Fixed for C# 7.3

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

        public List<MonitorProfile> GetActiveHardwareProfiles()
        {
            List<MonitorProfile> profiles = new List<MonitorProfile>();

            NativeMethods.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, delegate (IntPtr hMonitor, IntPtr hdc, IntPtr lprc, IntPtr lparam)
            {
                NativeStructures.MONITORINFOEX info = new NativeStructures.MONITORINFOEX();
                info.Size = Marshal.SizeOf(typeof(NativeStructures.MONITORINFOEX));

                if (NativeMethods.GetMonitorInfo(hMonitor, ref info))
                {
                    string deviceName = info.DeviceName.TrimEnd('\0');

                    // 1. Query Win32 for the true hardware friendly name string
                    NativeStructures.DISPLAY_DEVICE monitorDevice = new NativeStructures.DISPLAY_DEVICE();
                    monitorDevice.cb = Marshal.SizeOf(typeof(NativeStructures.DISPLAY_DEVICE));
                    string friendlyName = "Unknown Monitor";
                    if (NativeMethods.EnumDisplayDevices(deviceName, 0, ref monitorDevice, 0))
                    {
                        friendlyName = monitorDevice.DeviceString;
                    }

                    // 2. Query Shcore.dll for the exact unmanaged active metrics
                    NativeMethods.GetDpiForMonitor(hMonitor, MonitorDpiType.RawDpi, out uint rawDpi, out _);
                    NativeMethods.GetDpiForMonitor(hMonitor, MonitorDpiType.EffectiveDpi, out uint effectiveDpi, out _);

                    bool isGeneric = friendlyName != null &&
                                     friendlyName.IndexOf("Generic", StringComparison.OrdinalIgnoreCase) >= 0;

                    // 3. Populate live metrics, letting class defaults act as your safe fallback definitions!
                    MonitorProfile profile = new MonitorProfile();
                    profile.MonitorId = deviceName;
                    profile.MonitorName = friendlyName;
                    profile.HardwareDpi = rawDpi > 0 ? (double)rawDpi : 96.0;
                    profile.OSDpi = effectiveDpi > 0 ? (effectiveDpi / 96.0) : 1.0;
                    profile.IsGeneric = isGeneric;
                    profile.LastUsed = DateTime.Now;

                    profiles.Add(profile);
                }
                return true;
            }, IntPtr.Zero);

            return profiles;
        }
    }
}

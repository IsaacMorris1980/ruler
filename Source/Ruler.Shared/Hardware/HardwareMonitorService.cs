
using System;
using System.Collections.Generic;
using Ruler.Shared.Models;
using Ruler.Shared.Interfaces;

using static Ruler.Wpf.Common.NativeMethods;
using System.Runtime.InteropServices;
using static Ruler.Shared.Common.NativeStructures;
using static Ruler.Shared.Common.NativeMethods;
using static Ruler.Shared.Common.NativeEnums;
using static Ruler.Shared.Common.NativeConstants;
namespace Ruler.Shared.Hardware
{
    public class HardwareMonitorService : IHardwareMonitorService
    {
        public List<MonitorProfile> GetActiveHardwareProfiles()
        {
            var profiles = new List<MonitorProfile>();

            foreach (var screen in Screen.AllScreens)
            {
                DISPLAY_DEVICE monitorDevice = new DISPLAY_DEVICE();
                monitorDevice.cb = Marshal.SizeOf(monitorDevice);
                string friendlyName = "Unknown Monitor";

                if (EnumDisplayDevices(screen.DeviceName, 0, ref monitorDevice, 0))
                {
                    friendlyName = monitorDevice.DeviceString;
                }

                var centerPoint = new NativeMethods.POINT
                {
                    x = screen.Bounds.X + (screen.Bounds.Width / 2),
                    y = screen.Bounds.Y + (screen.Bounds.Height / 2)
                };

                IntPtr hMonitor = MonitorFromPoint(centerPoint, MONITOR_DEFAULTTONEAREST);

                GetDpiForMonitor(hMonitor, MonitorDpiType.RawDpi, out uint rawDpi, out _);
                GetDpiForMonitor(hMonitor, MonitorDpiType.EffectiveDpi, out uint effectiveDpi, out _);

                bool isGeneric = friendlyName != null &&
                               friendlyName.IndexOf("Generic", StringComparison.OrdinalIgnoreCase) >= 0;

                profiles.Add(new MonitorProfile()
                {
                    MonitorId = screen.DeviceName,
                    MonitorName = friendlyName,
                    HardwareDpi = (double)rawDpi,
                    OSDpi = effectiveDpi / 96.0,
                    IsGeneric = isGeneric,
                    LastUsed = DateTime.Now,
                    CalibratedDpi = 1.0
                });
            }

            return profiles;
        }

        public double GetDpiScale(IntPtr windowHandle)
        {
            var hMonitor = MonitorFromWindow(windowHandle, MONITOR_DEFAULTTONEAREST);
            if (GetDpiForMonitor(hMonitor, MonitorDpiType.EffectiveDpi, out uint dpiX, out _) == 0)
            {
                return dpiX / 96.0;
            }
            return 1.0;
        }

        public IntPtr GetMonitorHandleFromWindow(IntPtr windowHandle)
        {
            return MonitorFromWindow(windowHandle, MONITOR_DEFAULTTONEAREST);
        }
    }
}

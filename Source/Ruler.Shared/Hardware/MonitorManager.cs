using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using static Ruler.Shared.NativeMethods;
using static Ruler.Shared.NativeStructures;
using Ruler.Contracts.Models;
using Ruler.Contracts.Services.Persistance;
using static Ruler.Contracts.Interop.NativeStructures;
namespace Ruler.Shared
{
    public class MonitorManager
    {
        private readonly ILoggingService<MonitorManager> _loggingService;
        private readonly ISavingService _persistenceStrategy;
        public List<MonitorProfile> _monitorProfiles;
        public MonitorManager(ILoggingService<MonitorManager> loggingService, ISavingService persistenceStrategy)
        {
            _loggingService = loggingService;
            _persistenceStrategy = persistenceStrategy;
        }
        public void LoadMonitorProfiles()
        {
            _monitorProfiles = _persistenceStrategy.LoadMonitorProfiles();
        }
        public void SaveMonitorProfiles()
        {
            _persistenceStrategy.SaveMonitorProfiles(_monitorProfiles);
        }
        public MonitorProfile GetMonitorForRuler(string deviceName)
        {
            if (_monitorProfiles == null)
            {
                _monitorProfiles = GetActiveMonitors();
            }
            return _monitorProfiles.FirstOrDefault(m => m.MonitorId == deviceName);
        }
        public List<MonitorProfile> GetActiveMonitors()
        {
            List<MonitorProfile> profiles = new List<MonitorProfile>();
            // Instead of System.Windows.Forms.Screen, we use EnumDisplayMonitors 
            // to remain framework-agnostic.
            MonitorEnumProc callback = (IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData) =>
            {
                NativeStructures.MONITORINFOEX mi = new NativeStructures.MONITORINFOEX();
                mi.Size = Marshal.SizeOf(mi);
                if (GetMonitorInfo(hMonitor, ref mi))
                {
                    string deviceName = mi.DeviceName;
                    // Logic to get friendly name
                    NativeStructures.DISPLAY_DEVICE monitorDevice = new NativeStructures.DISPLAY_DEVICE();
                    monitorDevice.cb = Marshal.SizeOf(monitorDevice);
                    string friendlyName = "Unknown Monitor";
                    if (EnumDisplayDevices(deviceName, 0, ref monitorDevice, 0))
                    {
                        friendlyName = monitorDevice.DeviceString;
                    }
                    // Get DPI Information using shared NativeMethods
                    GetDpiForMonitor(hMonitor, MonitorDpiType.RawDpi, out uint rawDpi, out _);
                    GetDpiForMonitor(hMonitor, MonitorDpiType.EffectiveDpi, out uint effectiveDpi, out _);
                    bool isGeneric = friendlyName != null &&
                                   friendlyName.IndexOf("Generic", StringComparison.OrdinalIgnoreCase) >= 0;
                    profiles.Add(new MonitorProfile
                    {
                        MonitorId = deviceName,
                        MonitorName = friendlyName,
                        HardwareDpi = (double)rawDpi,
                        OSDpi = effectiveDpi / 96.0,
                        IsGeneric = isGeneric,
                        IsCalibrated = false,
                        LastUsed = DateTime.Now,
                        CalibratedDpi = 1.0
                    });
                }
                return true;
            };
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, IntPtr.Zero);
            return profiles;
        }
    }
}
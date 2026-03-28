using Ruler.Wpf.Common;
using Ruler.Wpf.Models;
using Ruler.Wpf.Services.Persistence.Strategy;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using Ruler.Shared.Interfaces;
using Ruler.Shared.Models;  

using static Ruler.Wpf.Common.NativeMethods;

namespace Ruler.Shared.Hardware
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
            _monitorProfiles = _persistenceStrategy.Load<MonitorProfile>();
        } 
        public void SaveMonitorProfiles()
        {
            _persistenceStrategy.Save<MonitorProfile>(_monitorProfiles);
        }
        public MonitorProfile GetMonitorForRuler(string deviceName)
        {
            if (_monitorProfiles == null)
            {
               _monitorProfiles = GetActiveMonitors();
            }
            var profile = _monitorProfiles.FirstOrDefault(m => m.MonitorId == deviceName);
            if (profile == null)
            {
                 return _monitorProfiles.FirstOrDefault(m => m.MonitorId == Screen.PrimaryScreen.DeviceName);
            }
            return profile;
        }
   
        public void UpdateMonitorProfile(MonitorProfile profile)
        {
            if (_monitorProfiles == null)
            {
                LoadMonitorProfiles();
            }
            var existing = _monitorProfiles.FirstOrDefault(m => m.MonitorId == profile.MonitorId);
            if (existing != null)
            {
                existing.MonitorName = profile.MonitorName;
                existing.IsGeneric = profile.IsGeneric;
                existing.IsCalibrated = profile.IsCalibrated;
                existing.HardwareDpi = profile.HardwareDpi;
                existing.OSDpi = profile.OSDpi;
                existing.CalibratedDpi = profile.CalibratedDpi;
                existing.LastUsed = DateTime.Now;

            }
            else
            {
                _monitorProfiles.Add(profile);
            }
        }
        /// <summary>
        /// Retrieves the DPI scale for a specific window handle.
        /// </summary>
        public double GetDpiScale(IntPtr windowHandle)
        {
            var hMonitor = NativeMethods.MonitorFromWindow(windowHandle, NativeMethods.MONITOR_DEFAULTTONEAREST);
            if (NativeMethods.GetDpiForMonitor(hMonitor, NativeMethods.MonitorDpiType.EffectiveDpi, out uint dpiX, out _) == 0)
            {
                return dpiX / 96.0;
            }
            return 1.0;
        }

        /// <summary>
        /// Determines if the window is currently positioned within the bounds of any connected monitor.
        /// </summary>
        public bool IsVisibleOnAnyMonitor(Window window)
        {
            var windowRect = new NativeMethods.Rect
            {
                Left = (int)window.Left,
                Top = (int)window.Top,
                Right = (int)(window.Left + window.Width),
                Bottom = (int)(window.Top + window.Height)
            };

            bool isVisible = false;

            // EnumDisplayMonitors uses a callback. We return 'false' to cancel further enumeration
            // once we've confirmed the window is visible on at least one display.
            NativeMethods.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (IntPtr hMonitor, IntPtr hdcMonitor, ref NativeMethods.Rect lprcMonitor, IntPtr dwData) =>
            {
                bool intersects = windowRect.Left < lprcMonitor.Right &&
                                 windowRect.Right > lprcMonitor.Left &&
                                 windowRect.Top < lprcMonitor.Bottom &&
                                 windowRect.Bottom > lprcMonitor.Top;

                if (intersects)
                {
                    isVisible = true;
                    return false; // This acts like a 'break' for the Win32 enumeration process
                }
                return true; // Continue to the next monitor
            }, IntPtr.Zero);

            return isVisible;
        }

        /// <summary>
        /// Checks if the window is visible and moves it to a safe default position if it is off-screen.
        /// </summary>
        public void EnsureVisibility(Window window)
        {
            if (!IsVisibleOnAnyMonitor(window))
            {
                // Fallback to a safe position (usually top-left of primary)
                window.Left = 100;
                window.Top = 100;
            }
        }

        /// <summary>
        /// Resizes the window to cover the entire virtual screen area (all monitors).
        /// </summary>
        public void SpanAllMonitors(Window window)
        {
            int minLeft = 0, minTop = 0, maxRight = 0, maxBottom = 0;

            NativeMethods.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (IntPtr hMonitor, IntPtr hdcMonitor, ref NativeMethods.Rect lprcMonitor, IntPtr dwData) =>
            {
                minLeft = Math.Min(minLeft, lprcMonitor.Left);
                minTop = Math.Min(minTop, lprcMonitor.Top);
                maxRight = Math.Max(maxRight, lprcMonitor.Right);
                maxBottom = Math.Max(maxBottom, lprcMonitor.Bottom);
                return true;
            }, IntPtr.Zero);

            window.Left = minLeft;
            window.Top = minTop;
            window.Width = maxRight - minLeft;
            window.Height = maxBottom - minTop;
        }
        /// <summary>
        /// Captures current position relative to the monitor the window is on.
        /// </summary>
        public MonitorPositionData GetMonitorRelativePosition(Window window)
        {
            var hwnd = new WindowInteropHelper(window).Handle;
            var hMonitor = NativeMethods.MonitorFromWindow(hwnd, NativeMethods.MONITOR_DEFAULTTONEAREST);

            var info = new NativeMethods.MONITORINFOEX();
            info.Size = Marshal.SizeOf(typeof(NativeMethods.MONITORINFOEX));

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

        /// <summary>
        /// Calculates new Virtual X/Y based on saved hardware ID.
        /// </summary>
        public Point GetVirtualPosition(MonitorPositionData savedData)
        {
            // Use System.Windows.Forms.Screen to find the current physical screens
            var targetScreen = System.Windows.Forms.Screen.AllScreens
                .FirstOrDefault(s => s.DeviceName == savedData.DeviceId);

            if (targetScreen != null)
            {
                return new Point(
                    targetScreen.Bounds.Left + savedData.RelativeX,
                    targetScreen.Bounds.Top + savedData.RelativeY
                );
            }

            // Fallback: If monitor is gone, return primary screen center
            return new Point(
                SystemParameters.PrimaryScreenWidth / 2 - 100,
                SystemParameters.PrimaryScreenHeight / 2 - 20
            );
        }
        public void SynchronizeProfiles(List<MonitorProfile> profiles)
        {
            var activeHardware = GetActiveMonitors();
            var activeIds = activeHardware.Select(h => h.MonitorId).ToList();

            // 1. Remove profiles for monitors that are no longer connected
            profiles.RemoveAll(p => !activeIds.Contains(p.MonitorId));

            // 2. Add or Update profiles based on current hardware
            foreach (var hardware in activeHardware)
            {
                var existingProfile = profiles.FirstOrDefault(p => p.MonitorId == hardware.MonitorId);

                if (existingProfile == null)
                {
                    profiles.Add(hardware);
                }
                else
                {
                    existingProfile.OsScale = hardware.OsScale;
                    existingProfile.MonitorDpi = hardware.MonitorDpi;
                    existingProfile.LastUsed = DateTime.Now;
                }
            }
        }

        private List<MonitorProfile> GetActiveMonitors()
        {
            var profiles = new List<MonitorProfile>();

            foreach (var screen in System.Windows.Forms.Screen.AllScreens)
            {
                string deviceName = screen.DeviceName;

                DISPLAY_DEVICE monitorDevice = new DISPLAY_DEVICE();
                monitorDevice.cb = Marshal.SizeOf(monitorDevice);
                string friendlyName = "Unknown Monitor";

                if (EnumDisplayDevices(deviceName, 0, ref monitorDevice, 0))
                {
                    friendlyName = monitorDevice.DeviceString;
                }

                // FIX: Use the center of the screen bounds to ensure we are looking up 
                // the DPI for the correct monitor, regardless of layout orientation.
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

                profiles.Add(new MonitorProfile
                {
                    MonitorId = deviceName,
                    MonitorName = friendlyName,
                    MonitorDpi = (double)rawDpi,
                    OsScale = effectiveDpi / 96.0,
                    IsGeneric = isGeneric,
                    IsCalibrated = false,
                    LastUsed = DateTime.Now,
                    CalibratedScale = 1.0
                });
            }

            return profiles;
        }
    }
}

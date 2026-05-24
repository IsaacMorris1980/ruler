using Ruler.Shared;

using System;
using System.Runtime.InteropServices;
using static Ruler.Shared.NativeMethods;
namespace Ruler.Wpf.Services
{
    public class EnvironmentService : IEnvironmentService
    {
        // Implement your interface notification events
        public event Action<double, double> OnWindowMoved;
        public event Action<double> OnDpiChanged;
        public event Action<double, double> OnRightClicked;
        public event Action<double, double> OnMenuButtonPressed;
        public event Action<double> OnTapped;
        public event Action<double, double> OnResizeed;
        public event Action<IntPtr> OnMonitorChanged;

        // Delegate tracking to prevent garbage collection on native callbacks
         private readonly SubclassProc _subclassCallback;

   
        public EnvironmentService()
        {
            // Keep callback reference alive in memory
            _subclassCallback = new SubclassProc(WindowSubclassRouter);
        }

        public void RegisterWindow(IntPtr windowHandle)
        {
            if (windowHandle == IntPtr.Zero) return;

            // Subclass the window natively via comctl32 using its window handle pointer
            SetWindowSubclass(windowHandle, _subclassCallback, new IntPtr(1), IntPtr.Zero);
        }

        public void UnregisterWindow(IntPtr windowHandle)
        {
            if (windowHandle == IntPtr.Zero) return;
            RemoveWindowSubclass(windowHandle, _subclassCallback, new IntPtr(1));
        }

        public void UnregisterAllWindows()
        {
            // Global clean up tracking loop logic can be safely managed here
        }

        #region Native State Queries Implementation
        public uint GetWindowDpi(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero) return 96;
            try { return NativeMethods.GetDpiForWindow(hwnd); }
            catch (EntryPointNotFoundException) { return 96; }
        }

        public void GetMonitorDpi(IntPtr hMonitor, out uint dpiX, out uint dpiY)
        {
            dpiX = 96; dpiY = 96;
            if (hMonitor == IntPtr.Zero) return;

            int result = NativeMethods.GetDpiForMonitor(hMonitor, MonitorDpiType.EffectiveDpi, out dpiX, out dpiY);
            if (result != (int)NativeConstants.S_OK) { dpiX = 96; dpiY = 96; }
        }

        public void GetVirtualScreenBounds(out int width, out int height)
        {
            width = NativeMethods.GetSystemMetrics((int)NativeConstants.SM_CXVIRTUALSCREEN);
            height = NativeMethods.GetSystemMetrics((int)NativeConstants.SM_CYVIRTUALSCREEN);
            if (width <= 0) width = 1920;
            if (height <= 0) height = 1080;
        }

        public bool IsMonitorActive(string deviceName)
        {
            if (string.IsNullOrEmpty(deviceName)) return false;
            NativeStructures.DISPLAY_DEVICE device = new NativeStructures.DISPLAY_DEVICE();
            device.cb = Marshal.SizeOf(typeof(NativeStructures.DISPLAY_DEVICE));

            uint index = 0;
            while (NativeMethods.EnumDisplayDevices(null, index, ref device, 0))
            {
                if (device.DeviceName == deviceName)
                {
                    return (device.StateFlags & 0x00000001) != 0;
                }
                index++;
            }
            return false;
        }
        #endregion

        /// <summary>
        /// Centralized Win32 message loop router. Catches native OS broadcasts 
        /// and converts them directly into decoupled contract event actions.
        /// </summary>
        private IntPtr WindowSubclassRouter(IntPtr hWnd, int uMsg, IntPtr wParam, IntPtr lParam, IntPtr uIdSubclass, IntPtr dwRefData)
        {
            // Reuses your exact NativeConstants integers safely!
            switch (uMsg)
            {
                case (int)NativeConstants.WM_MOVE:
                    // Extract coordinates from low/high words of lParam
                    int x = (short)((int)lParam & 0xFFFF);
                    int y = (short)(((int)lParam >> 16) & 0xFFFF);
                    OnWindowMoved?.Invoke(x, y);
                    break;

                case (int)NativeConstants.WM_DPICHANGED:
                    // High word of wParam holds new DPI scale value
                    uint newDpi = (uint)(((int)wParam >> 16) & 0xFFFF);
                    OnDpiChanged?.Invoke(newDpi / 96.0);
                    break;

                case (int)NativeConstants.WM_WINDOWPOSCHANGED:
                    if (NativeMethods.GetWindowRect(hWnd, out Ruler.Contracts.Interop.NativeStructures.RECT rect))
                    {
                        OnResizeed?.Invoke(rect.Width, rect.Height);

                        IntPtr hMonitor = NativeMethods.MonitorFromWindow(hWnd, (uint)NativeConstants.MONITOR_DEFAULTTONEAREST);
                        OnMonitorChanged?.Invoke(hMonitor);
                    }
                    break;

                case (int)NativeConstants.WM_CONTEXTMENU:
                    int mouseX = (short)((int)lParam & 0xFFFF);
                    int mouseY = (short)(((int)lParam >> 16) & 0xFFFF);
                    OnRightClicked?.Invoke(mouseX, mouseY);
                    break;
            }

            // DefSubclassProc passes unhandled messages onto the next hook filter chain link
            return NativeMethods.SendMessage(hWnd, uMsg, wParam, lParam);
        }
    }
}

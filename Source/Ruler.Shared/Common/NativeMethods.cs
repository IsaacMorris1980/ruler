using System;
using System.Runtime.InteropServices;
using static Ruler.Shared.NativeStructures;

namespace Ruler.Shared
{
    public static class NativeMethods
    {
        #region Win32 Interop Declarations
        public delegate IntPtr SubclassProc(IntPtr hWnd, int uMsg, IntPtr wParam, IntPtr lParam, IntPtr uIdSubclass, IntPtr dwRefData);

        [DllImport("comctl32.dll", SetLastError = true)]
        public static extern bool SetWindowSubclass(IntPtr hWnd, SubclassProc pfnSubclass, IntPtr uIdSubclass, IntPtr dwRefData);

        [DllImport("comctl32.dll", SetLastError = true)]
        public static extern bool RemoveWindowSubclass(IntPtr hWnd, SubclassProc pfnSubclass, IntPtr uIdSubclass);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

         [DllImport("user32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out Ruler.Contracts.Interop.NativeStructures.RECT lpRect);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);

         [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

         [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr MonitorFromRect(ref Ruler.Contracts.Interop.NativeStructures.RECT lprc, uint dwFlags);

         [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetSystemMetrics(int nIndex);

        [DllImport("shcore.dll", CharSet = CharSet.Auto)]
        public static extern int GetDpiForMonitor(IntPtr hmonitor, MonitorDpiType dpiType, out uint dpiX, out uint dpiY);

         [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetCursorPos(out Ruler.Contracts.Interop.NativeStructures.POINT lpPoint);

         [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr MonitorFromPoint(Ruler.Contracts.Interop.NativeStructures.POINT pt, uint dwFlags);

         [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr WindowFromPoint(Ruler.Contracts.Interop.NativeStructures.POINT point);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);


        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

         [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern uint GetDpiForWindow(IntPtr hwnd);

         [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);
       
        public delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref Ruler.Contracts.Interop.NativeStructures.RECT lprcMonitor, IntPtr dwData);
        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        
        public static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);
        public delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref Ruler.Contracts.Interop.NativeStructures.RECT lprcMonitor, IntPtr dwData);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int X,
            int Y,
            int cx,
            int cy,
            uint uFlags);
        #endregion
    }
}

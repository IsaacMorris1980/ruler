using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Wpf.Common
{
    public static class NativeMethods
    {
        // Constants for the Window Message (WM_SYSCOMMAND)
        public const int WM_SYSCOMMAND = 0x0112;
        public const int SC_SIZE = 0xF000;

        // Constants for the native HitTest codes (LPARAM)
        public const int HTLEFT = 10;
        public const int HTRIGHT = 11;
        public const int HTTOP = 12;
        public const int HTTOPLEFT = 13;
        public const int HTTOPRIGHT = 14;
        public const int HTBOTTOM = 15;
        public const int HTBOTTOMLEFT = 16;
        public const int HTBOTTOMRIGHT = 17;
        public const int WM_NCHITTEST = 0x0084;
        public const int HTCLIENT = 0x0001;
        public const int HTNOWHERE = 0x0000;
        // Resize HitTest codes (You MUST use these original codes for detection)
        public const int HTCAPTION = 0x0002;            // Hit Test Caption (allows dragging)
        public const int WM_WINDOWPOSCHANGED = 0x0047;  // Sent after the size/position is changed
        public const int WM_MOVE = 0x0003;
        public const int WM_RBUTTONDOWN = 0x0204;
        public const int WM_CONTEXTMENU = 0x007B;

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        // P/Invoke Declaration for sending messages to a window
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
        // This is exactly where you declare the external function:
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        // --- Win32 Interop Definitions for MonitorFromRect ---
        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromRect(ref RECT lprc, uint dwFlags);
    }
}

using System.Runtime.InteropServices;

namespace Ruler.Contracts.Interop
{
    public static class NativeStructures
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
            public int Width => Right - Left;
            public int Height => Bottom - Top;
        }

        public struct MonitorPositionData
        {
            public string DeviceId;
            public double RelativeX;
            public double RelativeY;
        }
    }
}

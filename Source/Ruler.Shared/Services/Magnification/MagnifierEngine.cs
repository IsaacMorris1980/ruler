using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Services.Magnification
{
    public class MagnifierEngine
    {
        // Native Win32 API Imports for ultra-fast screen blitting
        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

        private const int SRCCOPY = 0x00CC0020;

        /// <summary>
        /// Captures a square block of raw pixels directly centered around the current mouse cursor.
        /// </summary>
        public static Bitmap CaptureRegion(Point cursorPosition, int size)
        {
            Bitmap bmp = new Bitmap(size, size);

            IntPtr hdcScreen = GetDC(IntPtr.Zero);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                IntPtr hdcDest = g.GetHdc();

                // Calculate top-left starting corner to make sure capture area centers perfectly around cursor
                int srcX = cursorPosition.X - (size / 2);
                int srcY = cursorPosition.Y - (size / 2);

                // Hardware-accelerated memory copy of desktop pixels into our bitmap buffer
                BitBlt(hdcDest, 0, 0, size, size, hdcScreen, srcX, srcY, SRCCOPY);

                g.ReleaseHdc(hdcDest);
            }
            ReleaseDC(IntPtr.Zero, hdcScreen);

            return bmp;
        }
    }
}

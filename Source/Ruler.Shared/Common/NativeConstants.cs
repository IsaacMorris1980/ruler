using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Common
{
    public static class NativeConstants
    {
        public const int WM_NCRBUTTONUP = 0x00A5;
        public const int WM_MOVE = 0x0003;
        public const int WM_WINDOWPOSCHANGED = 0x0047;
        public const int WM_NCHITTEST = 0x0084;
        public const int WM_SYSCOMMAND = 0x0112;
        public const int WM_RBUTTONDOWN = 0x0204;
        public const int WM_CONTEXTMENU = 0x007B;
        public const int WM_DPICHANGED = 0x02E0;
        public const int WM_LBUTTONDOWN = 0x0201; // Left mouse button pressed
        public const int WM_LBUTTONUP = 0x0202; // Left mouse button released

        public const int HTNOWHERE = 0;
        public const int HTCLIENT = 1;
        public const int HTCAPTION = 2;
        public const int HTLEFT = 10;
        public const int HTRIGHT = 11;
        public const int HTTOP = 12;
        public const int HTTOPLEFT = 13;
        public const int HTTOPRIGHT = 14;
        public const int HTBOTTOM = 15;
        public const int HTBOTTOMLEFT = 16;
        public const int HTBOTTOMRIGHT = 17;

        public const int SC_SIZE = 0xF000;

        public const uint MONITOR_DEFAULTTONULL = 0;
        public const uint MONITOR_DEFAULTTOPRIMARY = 1;
        public const uint MONITOR_DEFAULTTONEAREST = 2;

        public const int SM_CXVIRTUALSCREEN = 78;
        public const int SM_CYVIRTUALSCREEN = 79;

        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_NOZORDER = 0x0004;
        public const uint SWP_NOACTIVATE = 0x0010;
    }
}

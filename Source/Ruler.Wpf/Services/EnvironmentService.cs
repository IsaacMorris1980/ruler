using Microsoft.Win32;

using Ruler.Wpf.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;

using static Ruler.Wpf.Common.NativeMethods;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

using Win=System.Windows;

namespace Ruler.Wpf.Services
{
    public class EnvironmentService : IEnvironmentService, IDisposable
    {
        public event EventHandler DisplayLayoutChanged;

        private const uint MONITOR_DEFAULTTONEAREST = 2;
        private const double StandardDpi = 96.0;
        private const int WM_WINDOWPOSCHANGED = 0x0047;
        private const int WM_NCHITTEST = 0x0084;
        private const int WM_NCRBUTTONUP = 0x00A5;
        private const int WM_CONTEXTMENU = 0x007B;
        private const int HTCAPTION = 2;
        private const int WM_DISPLAYCHANGE = 0x007E;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_MOUSEMOVE = 0x0200; // Added for drag tracking
        private const int WM_DPICHANGED = 0x02E0;
        private const int SWP_NOSIZE = 0x0001;
        private const int SWP_NOMOVE = 0x0002;

        // Win32 Metrics for Drag Threshold
        private const int SM_CXDRAG = 68;
        private const int SM_CYDRAG = 69;

        // Monitor Flags
        public const uint MONITOR_DEFAULTTONULL = 0;
        public const uint MONITOR_DEFAULTTOPRIMARY = 1;
       

        // System Metrics
        public const int SM_CXVIRTUALSCREEN = 78;
        public const int SM_CYVIRTUALSCREEN = 79;

        private Win.Point _startPos;
        private bool _isProcessing;
        private bool _movedBeyondThreshold;
        // Maps the Window Handle to the unique ID of the ViewMode
        private readonly Dictionary<IntPtr, HwndSourceHook> _hookCache = new Dictionary<IntPtr, HwndSourceHook>();
        public EnvironmentService()
        {

            SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
        }
        // 10 pixels is a standard "slop" value for touch screens to account for finger jitter
        private const double TapThreshold = 10.0;
        #region Win32_Imports
        private enum MonitorDpiType
        {
            EffectiveDpi = 0, // Includes OS scaling (125%, 150%, etc.)
            AngularDpi = 1,   // Based on viewing angle
            RawDpi = 2        // Literal hardware pixels per inch
        }
        private class WindowHookInfo
        {
            public Win.Window Window { get; set; }
            public Action<double, double> MoveCallback { get; set; }
            public Action<double> DpiCallback { get; set; }
            public Func<bool> IsLockedPredicate { get; set; }
            public Action<double, double> RightClickCallback { get; set; }
            public Action<double, double> ResizeCallback { get; set; }
            public Action<double> TapCallback { get; set; }
            public HwndSource HookSource { get; set; }
            public bool IsInMoveSizeLoop { get; set; }
            public Action<IntPtr> monitorChangedCallback { get; set; }
            public Func<bool> isPhysicalPredicate { get; set; }
        }

        #endregion
        private readonly Dictionary<IntPtr, WindowHookInfo> _activeHooks = new Dictionary<IntPtr, WindowHookInfo>();

        public double GetDpiScale(IntPtr window)
        {
            if (!_activeHooks.TryGetValue(window, out var info)) return 0.0;
            var s = NativeMethods.GetDpiForMonitor(window, (info.isPhysicalPredicate.Invoke()==true)?NativeMethods.MonitorDpiType.RawDpi: NativeMethods.MonitorDpiType.EffectiveDpi,out uint dipX,out uint dipY);
            return dipX / StandardDpi;
        }
        private void OnDisplaySettingsChanged(object sender, EventArgs e)
        {
            DisplayLayoutChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Captures current position relative to the monitor the window is on.
        /// </summary>
        public MonitorPositionData GetMonitorRelativePosition(Win.Window window)
        {
            var hwnd = new WindowInteropHelper(window).Handle;
            var hMonitor = NativeMethods.MonitorFromWindow(hwnd, NativeMethods.MONITOR_DEFAULTTONEAREST);

            var info = new NativeMethods.MONITORINFOEX();
            info.cbSize = Marshal.SizeOf(typeof(NativeMethods.MONITORINFOEX));

            if (NativeMethods.GetMonitorInfo(hMonitor, ref info))
            {
                return new MonitorPositionData
                {
                    DeviceId = info.szDevice,
                    RelativeX = window.Left - info.rcMonitor.Left,
                    RelativeY = window.Top - info.rcMonitor.Top
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

        public void Dispose()
        {
            SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;
        }

        public void RegisterWindow(
             Win.Window window,
             Action<double, double> moveCallback,
             Action<double> dpiCallback,
             Func<bool> isLockedPredicate,
             Action<double, double> onRightClicked,
             Action<double, double> onMenuButtonPressed,
             Action<double> onTap,
             Action<double, double> resizeCallback,
             Action<IntPtr> monitorChangedCallback,
             Func<bool> isPhysicalPredicate)
        {
           var helper = new WindowInteropHelper(window);
            IntPtr hwnd = helper.EnsureHandle();

            var hookInfo = new WindowHookInfo
            {
                Window = window,
                MoveCallback = moveCallback,
                DpiCallback = dpiCallback,
                IsLockedPredicate = isLockedPredicate,
                RightClickCallback = onRightClicked,
                ResizeCallback = resizeCallback,
                TapCallback = onTap,
                monitorChangedCallback=monitorChangedCallback,
                 isPhysicalPredicate=isPhysicalPredicate,
                HookSource = HwndSource.FromHwnd(hwnd)
            };

            hookInfo.HookSource.AddHook(WndProc);
            _activeHooks[hwnd] = hookInfo;
        }
     
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (!_activeHooks.TryGetValue(hwnd, out var info)) return IntPtr.Zero;

            switch (msg)
            {
                case WM_NCHITTEST:
                    if (info.IsLockedPredicate.Invoke() == true) return IntPtr.Zero;
                    handled = true;
                    return new IntPtr(HTCAPTION);



                case WM_WINDOWPOSCHANGED:
                    WINDOWPOS wp = (WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(WINDOWPOS));

                    // 1. Handle Resize (only if SWP_NOSIZE is NOT set)
                    if ((wp.flags & SWP_NOSIZE) == 0)
                    {
                        Vector dipSize = ConvertWidthHeightToDips(info.HookSource, wp.cx, wp.cy);
                        info.Window.Width = dipSize.X;
                        info.Window.Height = dipSize.Y;
                        info.ResizeCallback?.Invoke(dipSize.X, dipSize.Y);
                    }

                    // 2. Handle Move (only if SWP_NOMOVE is NOT set)
                    if ((wp.flags & SWP_NOMOVE) == 0)
                    {
                        Point dipPos = ConvertLeftAndTopToDips(info.HookSource, wp.x, wp.y);
                        info.Window.Left = dipPos.X;
                        info.Window.Top = dipPos.Y;
                        info.MoveCallback?.Invoke(dipPos.X, dipPos.Y);
                    }
                    break;

                case WM_NCRBUTTONUP:
                    GetCursorPos(out POINT p);
                    info.RightClickCallback?.Invoke(p.x, p.y);
                    handled = true;
                    break;

                case WM_DPICHANGED:
                    IntPtr suggestion = NativeMethods.MonitorFromWindow(info.HookSource.Handle, MONITOR_DEFAULTTONEAREST);
                    double newDpi=1.0;
                    if (info.isPhysicalPredicate.Invoke() == true)
                    {
                        var physicaldpi = NativeMethods.GetDpiForMonitor(suggestion, NativeMethods.MonitorDpiType.RawDpi, out uint dpiX, out _);
                    }
                    else
                    {
                        var suggestdpi = NativeMethods.GetDpiForMonitor(suggestion, NativeMethods.MonitorDpiType.EffectiveDpi, out uint dpiX, out _);
                    }
                    info.DpiCallback?.Invoke(newDpi);
                    break;
            }

            return IntPtr.Zero;
        }



        /// <summary>       
        /// Positions the window to span every available pixel across all monitors.
        /// </summary>
        public void SpanAllMonitors(System.Windows.Window window)
        {
            if (window == null) return;

            // Ensure window is in a state that allows spanning
            window.WindowStyle = Win.WindowStyle.None;
            window.ResizeMode = Win.ResizeMode.NoResize;
            window.WindowState = Win.WindowState.Normal;

            // VirtualScreen covers the bounding box of all displays
            window.Left = Win.SystemParameters.VirtualScreenLeft;
            window.Top = Win.SystemParameters.VirtualScreenTop;
            window.Width = Win.SystemParameters.VirtualScreenWidth;
            window.Height = Win.SystemParameters.VirtualScreenHeight;
        }
        public void UnregisterWindow(IntPtr window)
        {
            if (window != null && _hookCache.TryGetValue(window, out var hook))
            {
                //var hwnd = new WindowInteropHelper(window).Handle;
                if (window != IntPtr.Zero) HwndSource.FromHwnd(window)?.RemoveHook(hook);
                if (_hookCache.Count() > 1)
                {
                    _hookCache.Remove(window);
                }
                else
                {
                    _hookCache.Clear();
                }
            }
        }
        private Point ConvertLeftAndTopToDips(HwndSource source,int left,int top)
        {
            
            if (source != null && source.CompositionTarget != null)
            {
                Matrix deviceToDip = source.CompositionTarget.TransformFromDevice;
                Win.Point topLeft = deviceToDip.Transform(new Win.Point(left, top));
                return topLeft;

                // Now you have size.X, size.Y in DIPs
            }
            return new Point(0,0);
        }
        private Vector ConvertWidthHeightToDips(HwndSource source, int pixelWidth, int pixelHeight)
        { 
            if (source != null && source.CompositionTarget != null)
            {
                Matrix deviceToDip = source.CompositionTarget.TransformFromDevice;
                Win.Vector sizeVector = deviceToDip.Transform(new Win.Vector(pixelWidth, pixelHeight));
                return sizeVector;
                // Now you have size.X, size.Y in DIPs
            }
            return new Vector(400,75);
        }
       

        /// <summary>
        /// Returns the literal hardware DPI if you need to bypass OS scaling.
        /// </summary>
        public double GetPhysicalDpi(Visual visual)
        {
            Win.Point screenPoint = visual.PointToScreen(new Win.Point(0, 0));
            POINT pt = new POINT { x = (int)Math.Round(screenPoint.X), y = (int)Math.Round(screenPoint.Y) };
            IntPtr hMonitor = MonitorFromPoint(pt, MONITOR_DEFAULTTONEAREST);

            if (hMonitor != IntPtr.Zero)
            {
                GetDpiForMonitor(hMonitor, NativeMethods.MonitorDpiType.RawDpi, out uint dpiX, out _);
                return dpiX;
            }
            return StandardDpi;
        }
        /// <summary>
        /// Checks if any part of the window is currently visible on any connected monitor.
        /// </summary>
        public bool IsVisibleOnAnyMonitor(Win.Window window)
        {
            Win.Rect windowRect = new Win.Rect(window.Left, window.Top, window.Width, window.Height);

            return Screen.AllScreens.Any(screen =>
            {
                // Convert WinForms Screen bounds to WPF DIPs
                double dpi = GetDpiScale(new WindowInteropHelper(window).Handle);
                Win.Rect screenRect = new Win.Rect(
                    screen.Bounds.X / dpi,
                    screen.Bounds.Y / dpi,
                    screen.Bounds.Width / dpi,
                    screen.Bounds.Height / dpi);

                return windowRect.IntersectsWith(screenRect);
            });
        }
        public void EnsureVisibility(Win.Window window)
        {
            if (!IsVisibleOnAnyMonitor(window))
            {
                // If off-screen, reset to primary monitor
                window.Left = 100;
                window.Top = 100;
            }
        }

        //public void WatchWindow(IntPtr hwnd, Guid vmId)
        //{
        //    if (hwnd == IntPtr.Zero || _windowToVmMap.ContainsKey(hwnd)) return;

        //    var source = HwndSource.FromHwnd(hwnd);
        //    if (source == null) return;

        //    _windowToVmMap.Add(hwnd, vmId);

        //    HwndSourceHook hook = (IntPtr hwndHook, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) =>
        //    {
        //        switch (msg)
        //        {
        //            case WM_NCHITTEST:
        //                if (!_windowToVmMap.ContainsKey(hwnd)) break;
        //                // Trick Windows into thinking the body is the title bar (HTCAPTION) if not locked
        //                if (locked.IsLocked)
        //                {
        //                    handled = true;
        //                    return new IntPtr(HTCAPTION);
        //                }
        //                break;
        //            case WM_DPICHANGED:
        //                if (!_windowToVmMap.ContainsKey(hwnd)) break;
        //                var _currentDpi = (short)(wParam.ToInt32() & 0xFFFF);
        //                // 2. The lParam contains a pointer to a RECT with suggested size/position
        //                RECT suggestedRect = Marshal.PtrToStructure<RECT>(lParam);
        //                SystemDPIChangedMessage dpimsg = new SystemDPIChangedMessage(
        //                  _currentDpi, _windowToVmMap[hwnd]);
        //                _messageHubService.Publish(dpimsg);
        //                ConverttoDips(hwnd, suggestedRect.Left, suggestedRect.Top, suggestedRect.Right - suggestedRect.Left, suggestedRect.Bottom - suggestedRect.Top);
        //                break;
        //            case WM_DISPLAYCHANGE:
        //                Win.Window window = ReturnWindowFromIntPtr(hwnd);
        //                foreach (KeyValuePair<IntPtr, Guid> pair in _windowToVmMap)
        //                {
        //                    IsVisibleOnAnyMonitor(ReturnWindowFromIntPtr(pair.Key));
        //                }
        //                if (window == null) break;
        //                if (window.Name == "AllPages")
        //                {
        //                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
        //                    {
        //                        SpanAllMonitors(window);
        //                    }));
        //                }
        //                break;
        //            case WM_NCRBUTTONUP:
        //                // We DON'T set handled = true here. 
        //                // If we let this pass, Windows will automatically generate WM_CONTEXTMENU.
        //                break;

        //            case WM_CONTEXTMENU:
        //                // This is the "Gold Standard" for menus. 
        //                // It covers Right Click (Client), Right Click (NC/HTCAPTION), and the Menu Key.
        //                int x = (short)((int)lParam & 0xFFFF);
        //                int y = (short)((int)lParam >> 16);

        //                // If lParam is -1, it means the Menu key was pressed (keyboard)
        //                if (x == -1 && y == -1)
        //                {
        //                    Win.Window _windows = ReturnWindowFromIntPtr(hwnd);
        //                    OpenMenuChangedMessage menuMsg = new OpenMenuChangedMessage(_windows.Left + 20, _windows.Top + 20, _windowToVmMap[hwnd]);
        //                    _messageHubService.Publish(menuMsg);
        //                }
        //                else
        //                {
        //                    OpenMenuChangedMessage menuMsg = new OpenMenuChangedMessage(x, y, _windowToVmMap[hwnd]);
        //                    _messageHubService.Publish(menuMsg);
        //                }

        //                handled = true; // Mark as handled so the default system menu doesn't show
        //                break;
        //            case WM_LBUTTONDOWN:
        //                _isProcessing = true;
        //                _movedBeyondThreshold = false;
        //                Win.Window _window = ReturnWindowFromIntPtr(hwnd);
        //                _startPos = _window.PointToScreen(new Win.Point(0, 0));
        //                break;
        //            case WM_LBUTTONUP:
        //                if (_isProcessing)
        //                {
        //                    Win.Window _win = ReturnWindowFromIntPtr(hwnd);
        //                    Win.Point endPos = _win.PointToScreen(new Win.Point(0, 0));
        //                    double deltaX = Math.Abs(endPos.X - _startPos.X);
        //                    double deltaY = Math.Abs(endPos.Y - _startPos.Y);
        //                    if (deltaX > TapThreshold || deltaY > TapThreshold)
        //                    {
        //                        _movedBeyondThreshold = true;
        //                    }
        //                    // If we didn't move the window significantly, it's a tap
        //                    if (!_movedBeyondThreshold)
        //                    {
        //                        TappedChangedMessage onTap = new TappedChangedMessage(true, _windowToVmMap[hwnd], _startPos.X);
        //                        _messageHubService.Publish(onTap);
        //                    }
        //                    _isProcessing = false;
        //                }
        //                break;
        //        }
        //        return IntPtr.Zero;
        //    };

        //    source.AddHook(hook);
        //    _hookCache.Add(hwnd, hook);
        //}
        //public void UnregisterWindow(IntPtr window)
        //{
        //    if (window == null) return;
        //    _activeHooks.Remove(window);


        //}
        public void UnregisterAllWindows()
        {
            var hwnds = _activeHooks.Keys.ToList();
            foreach (var hwnd in hwnds)
            {
                UnregisterWindow(hwnd);
            }
            _activeHooks.Clear();
        }
        private Win.Window ReturnWindowFromIntPtr(IntPtr hwnd)
        {
            var source = HwndSource.FromHwnd(hwnd);
            Win.Window win = source?.RootVisual as Win.Window;
            return win;
        }
    }
}

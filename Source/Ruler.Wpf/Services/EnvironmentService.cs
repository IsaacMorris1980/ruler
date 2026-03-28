using Microsoft.Win32;

using Ruler.Wpf.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;

using static Ruler.Wpf.Common.NativeMethods;

using Win = System.Windows;

namespace Ruler.Wpf.Services
{
    public class EnvironmentService : IEnvironmentService, IDisposable
    {
        public event EventHandler DisplayLayoutChanged;

        private Win.Point _startPos;
        private bool _isProcessing;
        private bool _movedBeyondThreshold;
        private ILoggingService<EnvironmentService> _logger;    
        // Maps the Window Handle to the unique ID of the ViewMode
        private readonly Dictionary<IntPtr, HwndSourceHook> _hookCache = new Dictionary<IntPtr, HwndSourceHook>();
        private readonly Dictionary<IntPtr, WindowHookInfo> _activeHooks = new Dictionary<IntPtr, WindowHookInfo>();
        private const double TapThreshold = 10.0;

        public EnvironmentService(ILoggingService<EnvironmentService> logger)
        {
            logger = logger ?? throw new ArgumentNullException(nameof(logger));
            SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
        }
        // 10 pixels is a standard "slop" value for touch screens to account for finger jitter
       
 
        public double GetDpiScale(IntPtr window)
        {
            if (!_activeHooks.TryGetValue(window, out var info)) return 0.0;
            var s = NativeMethods.GetDpiForMonitor(window, (info.isPhysicalPredicate.Invoke()==true)?NativeMethods.MonitorDpiType.RawDpi: NativeMethods.MonitorDpiType.EffectiveDpi,out uint dipX,out uint dipY);
            return dipX / 96;
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

        public void Dispose()
        {
            SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;
        }
   

        public void RegisterWindow(
             Win.Window window)
        {
           var helper = new WindowInteropHelper(window);
            IntPtr hwnd = helper.EnsureHandle();

       
        }
     
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (!_activeHooks.TryGetValue(hwnd, out var info)) return IntPtr.Zero;
            int x=0;
            int y=0;
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
                        OnResizeed?.Invoke(dipSize.X, dipSize.Y);
                    }

                    // 2. Handle Move (only if SWP_NOMOVE is NOT set)
                    if ((wp.flags & SWP_NOMOVE) == 0)
                    {
                        Point dipPos = ConvertLeftAndTopToDips(info.HookSource, wp.x, wp.y);
                        OnWindowMoved?.Invoke(dipPos.X, dipPos.Y);
                    }
                    break;
                case WM_NCRBUTTONUP:

                    // 1. Extract coordinates from lParam
                    // Low order word is X, High order word is Y
                     x = (int)(short)((uint)lParam & 0xFFFF);
                      y = (int)(short)(((uint)lParam >> 16) & 0xFFFF);

                    // 2. Check if this was a keyboard-triggered event
                    // Windows sends -1, -1 in lParam for keyboard context menu requests
                    if (x == -1 && y == -1)
                    {
                        // For keyboard events, wParam is often the HWND of the window
                        if (wParam == IntPtr.Zero) break;

                        HwndSource source = HwndSource.FromHwnd(wParam);
                        Window mainWindow = source?.RootVisual as Window;

                        if (mainWindow != null)
                        {
                            // Offset from the top-left of the window for keyboard users
                            OnMenuButtonPressed?.Invoke(mainWindow.Left + 20, mainWindow.Top + 20);
                        }
                    }
                    else
                    {
                        // 3. It's a real mouse click. 
                        // We use the coordinates extracted from lParam for accuracy 
                        // relative to the moment the button was actually released.
                        OnRightClicked?.Invoke(x, y);
                    }

                    handled = true;
                    break;
                case WM_DPICHANGED:
                    IntPtr suggestion = NativeMethods.MonitorFromWindow(info.HookSource.Handle, MONITOR_DEFAULTTONEAREST);
                  
                    if (info.isPhysicalPredicate.Invoke() == true)
                    {
                        var physicaldpi = NativeMethods.GetDpiForMonitor(suggestion, NativeMethods.MonitorDpiType.RawDpi, out uint dpiX, out _);
                        OnDpiChanged?.Invoke(dpiX);
                    }
                    else
                    {
                        var suggestdpi = NativeMethods.GetDpiForMonitor(suggestion, NativeMethods.MonitorDpiType.EffectiveDpi, out uint dpiX, out _);
                        OnDpiChanged?.Invoke(dpiX/96);
                    }
                    break;
                case WM_LBUTTONDOWN:
                    _isProcessing = true;
                    _movedBeyondThreshold = false;
                    Win.Window _window = ReturnWindowFromIntPtr(hwnd);
                    if (_window != null)
                    {
                        _startPos = _window.PointToScreen(new Win.Point(0, 0));
                    }
                    break;
                case WM_LBUTTONUP:
                    if (_isProcessing)
                    {
                        Win.Window _win = ReturnWindowFromIntPtr(hwnd);
                        if (_win != null)
                        {
                            Win.Point endPos = _win.PointToScreen(new Win.Point(0, 0));
                            double deltaX = Math.Abs(endPos.X - _startPos.X);
                            double deltaY = Math.Abs(endPos.Y - _startPos.Y);
                            if (deltaX > TapThreshold || deltaY > TapThreshold)
                            {
                                _movedBeyondThreshold = true;
                            }
                            // If we didn't move the window significantly, it's a tap
                            if (!_movedBeyondThreshold)
                            {
                             
                            }
                            _isProcessing = false;
                        }
                        else
                        { 
                        UnregisterWindow(hwnd);
                        }
                    }
                    break;
                case WM_CONTEXTMENU:
                    // This is the "Gold Standard" for menus. 
                    // It covers Right Click (Client), Right Click (NC/HTCAPTION), and the Menu Key.
                    x = (short)((int)lParam & 0xFFFF);
                     y = (short)((int)lParam >> 16);

                    // If lParam is -1, it means the Menu key was pressed (keyboard)
                    if (x == -1 && y == -1)
                    {
                        Win.Window _windows = ReturnWindowFromIntPtr(hwnd);
                       
                    }
                    else
                    {
                       
                    }

                    handled = true; // Mark as handled so the default system menu doesn't show
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
            if (window == IntPtr.Zero) return;

            if (_activeHooks.TryGetValue(window, out var info))
            {
                info.HookSource?.RemoveHook(WndProc);
                _activeHooks.Remove(window);
            }

            _hookCache.Remove(window);
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
            return 96;
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
        private static readonly bool _isModernDpiSupported = CheckDpiSupport();

        private static bool CheckDpiSupport()
        {
            IntPtr user32 = NativeMethods.GetModuleHandle("user32.dll");
            return NativeMethods.GetProcAddress(user32, "GetDpiForWindow") != IntPtr.Zero;
        }
    }
}

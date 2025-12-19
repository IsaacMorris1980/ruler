using Ruler.Wpf.Common;
using Ruler.Wpf.Services;
using Ruler.Wpf.ViewModels;

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace Ruler.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {    
        private RulerViewModel _viewModel;    
        private ILoggingService _loggingService;    
        // Points used for calculating delta movements.
        private Point _startPoint;     
        // Flag to track if a drag operation has started
        private bool _isDragging = false;
      

        public MainWindow(RulerViewModel viewModel, ILoggingService loggingService)
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.SourceInitialized += MainWindow_SourceInitialized;
            this.DataContext = viewModel;
            _viewModel = viewModel; 
            _loggingService = loggingService;
            this.Loaded += MainWindow_Loaded;
           NativeMethods.RECT rulerarea = GetWindowRectFromWpf();
            bool isVisible = IsWindowVisible(rulerarea);
            if (!isVisible)
            {
                _loggingService.LogInfo("Ruler is NOT visible on any monitor.");
                _viewModel.DisplayedLocation = new Point(0, 0);
                _viewModel.Left = 0;
                _viewModel.Top = 0;
            }
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }
        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // When AutoScale is enabled via the menu, calculate and apply the system scale factor.
            if (e.PropertyName == nameof(RulerViewModel.IsAutoScaled) && _viewModel.IsAutoScaled)
            {
                ApplySystemDpiScale();
            }
        }
        private void ApplySystemDpiScale()
        {
            // Use PresentationSource to reliably get DPI scaling in WPF
            if (PresentationSource.FromVisual(this) is HwndSource source)
            {
                // M11 is the X-axis scaling factor (e.g., 1.25 for 125%)
                double dpiScaleFactor = source.CompositionTarget.TransformToDevice.M11;

                // Push the system DPI scale to the ViewModel
                _viewModel.SetAutoScaleFactor(dpiScaleFactor);
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
     
                if (this.DataContext is RulerViewModel viewModel && !viewModel.IsInitialized)
                {
                    
                    this.LocationChanged -= Window_LocationChanged;
                  //  ApplyDpiAwareMarginFix();
                    // Set the initialization flag

                    _viewModel = this.DataContext as RulerViewModel;
                    Console.WriteLine($"Width: {this.Width}, Height: {this.Height}");
                    Console.WriteLine($"RulerViewModel Width: {viewModel.Width}, Height: {viewModel.Height}");
                    _loggingService.LogInfo("MainWindow loaded. Initializing size and position.");
                    // Use the ViewModel's stored DIU values directly.
                    // WPF handles the DPI scaling automatically.
                  
                    this.Left = viewModel.Left;
                    this.Top = viewModel.Top;
     
                    
                    viewModel.IsInitialized = true;
                    this.LocationChanged += Window_LocationChanged;
                    Console.WriteLine($"Canvas Actual Height: {RulerCanvas.ActualHeight}, Actual Width: {RulerCanvas.ActualWidth}");
                  
                }
            }
            catch (Exception ex)
            {
                var a = ex.Message;
            }
            _isFullyLoaded = true;

        }
      
        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            // Get the window handle and set up the message loop override
            WindowInteropHelper helper = new WindowInteropHelper(this);
            HwndSource source = HwndSource.FromHwnd(helper.Handle);
           // 1. Hook up the window message handler for drag, lock, and move events
            source?.AddHook(HwndHook);          
            source?.AddHook(WndProc);
            // 2. Subscribe to the DpiChanged event to fix the vertical ruler margin when the DPI changes
            //  source.DpiChanged += Source_DpiChanged;
        }
        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {          
            if (msg ==NativeMethods.WM_MOVE || msg ==NativeMethods.WM_WINDOWPOSCHANGED)
            {    
                Window window = (Window)HwndSource.FromHwnd(hwnd).RootVisual;
                if (window != null)
                {
                    // This is the custom method that fixes the location reporting issue
                    GetDIPLocationFromWin32Rect(hwnd, window);
                }
            }

            return IntPtr.Zero;
        }
       
        private void GetDIPLocationFromWin32Rect(IntPtr hwnd, Window window)
        {
            // 1. Get the window's physical location in screen pixels using the native GetWindowRect
            if (Ruler.Wpf.Common.NativeMethods.GetWindowRect(hwnd, out NativeMethods.RECT rect))
            {
                HwndSource source = PresentationSource.FromVisual(window) as HwndSource;

                if (source != null)
                {
                    // 2. Get the Transformation Matrix (Device Pixels -> DIPs)
                    if (source.CompositionTarget != null)
                    {
                        // TransformFromDevice maps screen pixels to WPF units (DIPs)
                        Matrix matrix = source.CompositionTarget.TransformFromDevice;

                        // 3. Apply the transformation matrix to the Left/Top pixel coordinates
                        Point locationInDIP = matrix.Transform(new Point(rect.Left, rect.Top));

                        double actualLeft = locationInDIP.X;
                        double actualTop = locationInDIP.Y;
                      

                        // 4. Manually write the corrected position back to the ViewModel properties.
                        _viewModel.UpdateLocation(actualLeft, actualTop);
                    }
                }
                // Fallback in case HwndSource is not available
                else
                {
                    Console.WriteLine($"HwndSource not found. Falling back to WPF: Left={window.Left}, Top={window.Top}");
                    _viewModel.UpdateLocation(window.Left, window.Top);
                }
            }
        }
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg ==NativeMethods.WM_NCHITTEST)
            {
               if (_viewModel != null && _viewModel.IsLocked)
                {
                  // 4. Override the result to HTCAPTION (2). 
                        // This tells Windows to treat the mouse click as a drag action,
                        // disabling the resize but allowing the window to be moved.
                        handled = true;
                        return new IntPtr(Ruler.Wpf.Common.NativeMethods.HTCAPTION);
                    }
                

                // If not locked, or not over a resize area, return the original result.
                return IntPtr.Zero;
            }

            // Return 0 for default processing for all other messages
            return IntPtr.Zero;
        }
        private bool _isFullyLoaded = false;
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!_isFullyLoaded || _viewModel is null)
            {
                return;
            }
            _viewModel.SetRulerDimensions(e.NewSize.Width, e.NewSize.Height);
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            if (_viewModel != null)
            {
                Window window = (Window)sender;
               

                IntPtr windowHandle = new WindowInteropHelper(window).Handle;

                // 1. Call the native Windows API to get the true screen position in pixels
                if (Ruler.Wpf.Common.NativeMethods.GetWindowRect(windowHandle, out NativeMethods.RECT rect))
                {
                    // 2. Convert the Win32 pixel coordinates to WPF's Device Independent Pixels (DIPs)
                    PresentationSource source = PresentationSource.FromVisual(window);

                    if (source != null && source.CompositionTarget != null)
                    {
                        Matrix matrix = source.CompositionTarget.TransformFromDevice;

                        // Apply the transformation matrix to the Left/Top pixel coordinates
                        Point locationInDIP = matrix.Transform(new Point(rect.Left, rect.Top));

                        double actualLeft = locationInDIP.X;
                        double actualTop = locationInDIP.Y;                                             

                        // 3. Manually write the window's current screen position back to the ViewModel properties.
                        _viewModel.UpdateLocation(actualLeft, actualTop);
                        //_viewModel.LocationX = actualLeft;
                        //_viewModel.LocationY = actualTop;

                    }
                }
                // If the Win32 call fails, fall back to WPF properties (will likely still be 0/78)
                else
                {
                    Console.WriteLine($"Win32 API Failed. Falling back to WPF: Left={window.Left}, Top={window.Top}");
                    _viewModel.UpdateLocation(window.Left, window.Top);
                    //_viewModel.LocationX = window.Left;
                    //_viewModel.LocationY = window.Top;
                }
            }
            if (_viewModel.IsAutoScaled)
            {
                ApplySystemDpiScale();
            }
        }
        private const int MONITOR_DEFAULTTONULL = 0x00000000;

        private static bool IsWindowVisible(Ruler.Wpf.Common.NativeMethods.RECT rect)
        {
            // MonitorFromRect returns a handle (IntPtr) to the display monitor
            // that intersects the rectangle. We use MONITOR_DEFAULTTONULL (0)
            // so it returns NULL if the rectangle does not intersect any display monitor.
            IntPtr monitorHandle =NativeMethods.MonitorFromRect(ref rect, MONITOR_DEFAULTTONULL);

            // If the handle is not zero (NULL), a monitor was found, meaning the window is visible.
            return monitorHandle != IntPtr.Zero;
        }

        /// <summary>
        /// Helper to convert WPF position/size to native RECT.
        /// </summary>
        private NativeMethods.RECT GetWindowRectFromWpf()
        {
            return new NativeMethods.RECT
            {
                Left = (int)this.Left,
                Top = (int)this.Top,
                Right = (int)(this.Left + this.Width),
                Bottom = (int)(this.Top + this.Height)
            };
        }

        private void RulerCanvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && RulerCanvas.IsMouseCaptured)
            {
                Point currentPoint = e.GetPosition(RulerCanvas);
                if (!_isDragging && (Math.Abs(currentPoint.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                                     Math.Abs(currentPoint.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance))
                {
                    _isDragging = true;
                    RulerCanvas.ReleaseMouseCapture();
                    this.DragMove();
                }

            }
        }
       
        private void RulerCanvas_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            RulerCanvas.ReleaseMouseCapture();
            if (!_isDragging)
            {
                Point clickPoint = e.GetPosition(RulerCanvas);
                double position;
                if (_viewModel.IsVertical)
                {
                    position = clickPoint.Y;
                }
                else
                {
                    position = clickPoint.X;
                }
                // Update the ViewModel's guide line property
                _viewModel.SetGuideLinePosition(position);
                // Also ensure the line is visible
                _viewModel.IsGuideLineVisible = true;
            }

        }
        private void RulerCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(RulerCanvas);
            _isDragging = false;
            RulerCanvas.CaptureMouse();
        }
        private void RulerCanvas_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel != null && _viewModel.IsLocked)
            {
                // 1. Mark the event as handled to prevent it from propagating further up 
                //    to the Window/System level where it might be consumed.
                e.Handled = true;

                if (RulerCanvas.ContextMenu != null)
                {
                    // 2. Manually set the placement target to the canvas itself
                    RulerCanvas.ContextMenu.PlacementTarget = RulerCanvas;

                    // 3. Set the position of the menu to the current mouse click position
                    Point clickPoint = e.GetPosition(RulerCanvas);
                    RulerCanvas.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.AbsolutePoint;
                    RulerCanvas.ContextMenu.HorizontalOffset = clickPoint.X;
                    RulerCanvas.ContextMenu.VerticalOffset = clickPoint.Y;

                    // 4. Open the ContextMenu
                    RulerCanvas.ContextMenu.IsOpen = true;
                }
            }
        }
    }
    public class BooleanToVisibilityConverter : IValueConverter
    {
        // Converts boolean to visibility (true -> Visible, false -> Collapsed)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool booleanValue)
            {
                bool invert = false;

                // Check for the 'ConverterParameter' to invert the logic
                if (parameter != null && parameter.ToString().Equals("Invert", StringComparison.OrdinalIgnoreCase))
                {
                    invert = true;
                }

                // Apply the logic:
                // If invert is true, visible when booleanValue is false.
                // If invert is false, visible when booleanValue is true.
                Console.WriteLine(booleanValue);
                Console.WriteLine(invert);
                if (booleanValue != invert)
                {
                    Console.WriteLine("Visible");
                    return Visibility.Visible;
                }
            }
            Console.WriteLine("Collapsed");
            return Visibility.Collapsed;
        }

        // Converts visibility to boolean (not typically needed for UI binding)
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}

using Ruler.Wpf.Common;
using Ruler.Wpf.Services;
using Ruler.Wpf.ViewModels;

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Ruler.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private RulerViewModel _viewModel;
        private ILoggingService _loggingService;
        private IEnvironmentService _environmentService;
        // Points used for calculating delta movements.
        private Point _startPoint;
        // Flag to track if a drag operation has started
        private bool _isDragging = false;
        private bool _isGuidelineLocked = false;
       

        public MainWindow(RulerViewModel viewModel,IEnvironmentService service)
        {
            InitializeComponent();
            this.SourceInitialized += MainWindow_SourceInitialized;
            this.DataContext = viewModel;
            _viewModel = viewModel;
            _environmentService = service;




        }
        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            _environmentService.RegisterWindow(
     this,
     // moveCallback: Updates whenever the window moves
     moveCallback:(x, y) =>
     {
         _viewModel.Left = x;
         _viewModel.Top = y;
     },
     dpiCallback:(dpi) =>
     {
         if (_viewModel.IsAutoScaled)
         {
             _viewModel.ScaleFactor = dpi;
         }
     },
     isLockedPredicate:() => _viewModel.IsLocked,
     onRightClicked:(x, y) => { ShowMenu?.ShowAt(x, y, this); },
    onMenuButtonPressed:(x, y) => { ShowMenu?.ShowAt(x, y, this); },
     onTap:(double x) => { _viewModel.SetGuideLinePosition(x); },
     resizeCallback:(width, height) =>
        {
            _viewModel.Width = width;
            _viewModel.Height = height;
        },
    // NEW: monitorCallback (IntPtr hMonitor)
    monitorChangedCallback:(hMonitor) => {
        // This is where you handle logic specific to entering a new monitor
        // e.g., checking if the new monitor is ultra-wide, or updating
        // the ViewModel's "current monitor" bounds for snapping.
        var dpi = _environmentService.GetDpiScale(hMonitor);


        if (dpi != 0.0)
        {
            _viewModel.SystemDpiScale = dpi;
        }
        _viewModel.OnMonitorChanged(hMonitor);
    },
    isPhysicalPredicate:()=>_viewModel.IsPhysicalUnits
);

            this.Closed += (s, args) =>
            {
                this.SourceInitialized -= MainWindow_SourceInitialized;
            };
        }
        private bool _isFullyLoaded = false;

        private void UpdateMagnifier(Point pos)
        {
            if (RulerSurface == null || MagnifierVisual == null || MagnifierLens == null) return;
            double rulerHeight = RulerSurface.ActualHeight;
            double magnifierSize = rulerHeight;
            MagnifierVisual.Width = magnifierSize;
            MagnifierVisual.Height = rulerHeight;

            // 2. Position the lens
            double lensLeft = pos.X - (magnifierSize / 2);
            Canvas.SetLeft(MagnifierVisual, lensLeft);
            Canvas.SetTop(MagnifierVisual, 0);

            // 3. The Red Center Line (The "pointer" for the mouse)
           

            // 4. Configure Zoom Math
            double zoomFactor = 2.5;
            double sourceWidth = magnifierSize / zoomFactor;
            double sourceX = pos.X - (sourceWidth / 2);           
        }
        private void HideMagnifier()
        {
            if (MagnifierVisual != null) MagnifierVisual.Visibility = Visibility.Collapsed;
          
        }
        private void MainCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            HideMagnifier();
            // Only hide red line on leave; blue line stays if locked
            if (!_isGuidelineLocked)
            {
                Guideline.Visibility = Visibility.Collapsed;
            }
        }
    }
   
}

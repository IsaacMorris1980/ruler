using Ruler.Shared.Commands;
using Ruler.Shared.Enums;
using Ruler.Shared.Helpers;
using Ruler.Shared.Interfaces;
using Ruler.Shared.Models;
using Ruler.Shared.Services;
using Ruler.Wpf.ViewModels;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Ruler.Wpf.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Interop Constants for locking resize
        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_SIZE = 0xF000;
        private const int WM_SIZING = 0x0214;
        private HwndSource _hwndSource;

        // Accept the ViewModel via constructor injection (supplied by your factory)
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.CloseRequested += (s, e) => this.Close();

            // Setup Win32 Interop for Locking/Resize logic (View-specific concern)
            this.SourceInitialized += (s, e) =>
            {
                _hwndSource = PresentationSource.FromVisual(this) as HwndSource;
                _hwndSource?.AddHook(WndProc);
            };
        }

        // --- View-Specific Interop & Window Behaviors ---

        // Equivalent to WinForms WndProc (Blocks resizing when locked)
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // We can read lock status safely from DataContext if needed, 
            // or let the ViewModel expose a simple bool property.
            if (DataContext is MainViewModel vm && vm.IsLocked)
            {
                if (msg == WM_SIZING)
                {
                    handled = true;
                    return IntPtr.Zero;
                }
                if (msg == WM_SYSCOMMAND && (wParam.ToInt32() & 0xFFF0) == SC_SIZE)
                {
                    handled = true;
                    return IntPtr.Zero;
                }
            }
            return IntPtr.Zero;
        }

        // Native WPF window dragging on mouse down
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        // --- Custom Drawing / Rendering (View-specific presentation) ---

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (ActualWidth <= 0 || ActualHeight <= 0) return;
            if (!(DataContext is MainViewModel vm)) return;

            var tickPen = new Pen(Brushes.Black, 1);
            var textBrush = Brushes.Black;
            var fontTypeface = new Typeface("Segoe UI");
            double dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;

            var rulerRect = new Rect(0, 0, ActualWidth, ActualHeight);
            drawingContext.DrawRectangle(Brushes.LightSlateGray, tickPen, rulerRect);

            if (vm.IsVertical)
            {
                DrawVerticalRuler(drawingContext, tickPen, textBrush, fontTypeface, dpi);
            }
            else
            {
                DrawHorizontalRuler(drawingContext, tickPen, textBrush, fontTypeface, dpi);
            }
        }

        private void DrawHorizontalRuler(DrawingContext dc, Pen pen, Brush brush, Typeface typeface, double dpi)
        {
            bool dualSided = ActualHeight > 100;

            for (double x = 0; x <= ActualWidth; x += 10)
            {
                bool isMajor = (x % 50 == 0);
                bool isMedium = (x % 25 == 0);
                double tickLength = isMajor ? 12 : (isMedium ? 8 : 4);

                dc.DrawLine(pen, new Point(x, 0), new Point(x, tickLength));
                dc.DrawLine(pen, new Point(x, ActualHeight), new Point(x, ActualHeight - tickLength));

                if (isMajor && x > 0)
                {
                    var formattedText = new FormattedText(
                        x.ToString(),
                        System.Globalization.CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        typeface,
                        10,
                        brush,
                        dpi);

                    if (dualSided)
                    {
                        dc.DrawText(formattedText, new Point(x - (formattedText.Width / 2), 15));
                        dc.DrawText(formattedText, new Point(x - (formattedText.Width / 2), ActualHeight - 25));
                    }
                    else
                    {
                        dc.DrawText(formattedText, new Point(x - (formattedText.Width / 2), (ActualHeight / 2) - (formattedText.Height / 2)));
                    }
                }
            }
        }

        private void DrawVerticalRuler(DrawingContext dc, Pen pen, Brush brush, Typeface typeface, double dpi)
        {
            bool dualSided = ActualWidth > 100;

            for (double y = 0; y <= ActualHeight; y += 10)
            {
                bool isMajor = (y % 50 == 0);
                bool isMedium = (y % 25 == 0);
                double tickLength = isMajor ? 12 : (isMedium ? 8 : 4);

                dc.DrawLine(pen, new Point(0, y), new Point(tickLength, y));
                dc.DrawLine(pen, new Point(ActualWidth, y), new Point(ActualWidth - tickLength, y));

                if (isMajor && y > 0)
                {
                    var formattedText = new FormattedText(
                        y.ToString(),
                        System.Globalization.CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        typeface,
                        10,
                        brush,
                        dpi);

                    if (dualSided)
                    {
                        dc.DrawText(formattedText, new Point(22, y - (formattedText.Height / 2)));
                        dc.DrawText(formattedText, new Point(ActualWidth - 28, y - (formattedText.Height / 2)));
                    }
                    else
                    {
                        dc.DrawText(formattedText, new Point((ActualWidth / 2) - (formattedText.Width / 2), y - (formattedText.Height / 2)));
                    }
                }
            }
        }
    }
}

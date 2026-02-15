using Ruler.Wpf.ViewModels;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Ruler.Wpf.Services
{

    public interface IEnvironmentService
    {
        /// <summary>
        /// Gets the current DPI scaling factor for a specific window.
        /// </summary>
        double GetDpiScale(IntPtr window);
        /// <summary>
        /// Determines if the window is currently positioned within the bounds of any connected monitor.
        /// </summary>
        bool IsVisibleOnAnyMonitor(Window window);

        /// <summary>
        /// Checks if the window is visible and moves it to a safe default position if it is off-screen.
        /// </summary>
        void EnsureVisibility(Window window);

        /// <summary>
        /// Resizes the window to cover the entire virtual screen area (all monitors).
        /// </summary>
        void SpanAllMonitors(Window window);

        /// <summary>
        /// Hooks into the window message loop to handle dragging, DPI changes, and monitor updates.
        /// </summary>
        void RegisterWindow(
            Window window,
            Action<double, double> moveCallback,
            Action<double> dpiCallback,
            Func<bool> isLockedPredicate,
            Action<double, double> onRightClicked,
            Action<double, double> onMenuButtonPressed,
            Action<double> onTap,
            Action<double, double> resizeCallback,
            Action<IntPtr> monitorChangedCallback,
             Func<bool> isPhysicalPredicate);

        /// <summary>
        ///  hooks into the window message loop to handle dragging, DPI changes, and monitor updates.
        /// </summary>
        //void WatchWindow(IntPtr hwnd, Guid vmId);
        void UnregisterWindow(IntPtr window);
        void UnregisterAllWindows();
     
    }
}


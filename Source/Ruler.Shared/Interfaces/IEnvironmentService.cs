using Ruler.Wpf.ViewModels;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Ruler.Shared.Interfaces
{

    public interface IEnvironmentService
    {
        /// <summary>
        /// Hooks into the window message loop to handle dragging, DPI changes, and monitor updates.
        /// </summary>
        void RegisterWindow(
            Window window);
        
        /// <summary>
        ///  hooks into the window message loop to handle dragging, DPI changes, and monitor updates.
        /// </summary>
        //void WatchWindow(IntPtr hwnd, Guid vmId);
        void UnregisterWindow(IntPtr window);
        void UnregisterAllWindows();
        event Action<double, double> OnWindowMoved;
        event Action<double> OnDpiChanged;
        event Action<double, double> OnRightClicked;
        event Action<double, double> OnMenuButtonPressed;
        event Action<double> OnTapped;
        event Action<double, double> OnResizeed;
        event Action<IntPtr> OnMonitorChanged;
    }
}


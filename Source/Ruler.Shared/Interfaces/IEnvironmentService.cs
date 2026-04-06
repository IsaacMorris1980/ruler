using System;
namespace Ruler.Shared
{
    public interface IEnvironmentService
    {
        /// <summary>
        /// Hooks into the window message loop to handle dragging, DPI changes, and monitor updates.
        /// </summary>
        void RegisterWindow(
            IntPtr window);
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


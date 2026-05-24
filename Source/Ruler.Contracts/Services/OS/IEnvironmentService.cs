using System;
namespace Ruler.Shared
{
    public interface IEnvironmentService
    {
        #region Lifecycle & Window Hook Registration
        /// <summary>
        /// Hooks into the unmanaged window message loop using its HWND handle 
        /// to capture dragging, DPI changes, and monitor updates.
        /// </summary>
        void RegisterWindow(IntPtr windowHandle);

        /// <summary>
        /// Unhooks the window message loop sub-classing filter for a specific window handle.
        /// </summary>
        void UnregisterWindow(IntPtr windowHandle);

        /// <summary>
        /// Clears all active unmanaged window hooks on application teardown.
        /// </summary>
        void UnregisterAllWindows();
        #endregion

        #region Native OS State Queries
        /// <summary>
        /// Retrieves the current DPI scaling factor for a specific window handle.
        /// </summary>
        uint GetWindowDpi(IntPtr hwnd);

        /// <summary>
        /// Retrieves the raw or scaling DPI from an unmanaged monitor handle.
        /// </summary>
        void GetMonitorDpi(IntPtr hMonitor, out uint dpiX, out uint dpiY);

        /// <summary>
        /// Calculates absolute width and height sizes bounding the entire virtual desktop grid space.
        /// </summary>
        void GetVirtualScreenBounds(out int width, out int height);

        /// <summary>
        /// Loops hardware paths using display configurations to confirm device connection status.
        /// </summary>
        bool IsMonitorActive(string deviceName);
        #endregion

        #region Decoupled Message Pump Notification Events
        /// <summary>
        /// Fires when an unmanaged window position changes, passing the relative coordinates.
        /// </summary>
        event Action<double, double> OnWindowMoved;

        /// <summary>
        /// Fires when the OS broadcasts a display resolution or scaling change factor adjustment.
        /// </summary>
        event Action<double> OnDpiChanged;

        /// <summary>
        /// Fires on secondary mouse click message triggers, passing raw coordinate markers.
        /// </summary>
        event Action<double, double> OnRightClicked;

        /// <summary>
        /// Fires when systemic navigation or menu commands intercept the pipeline.
        /// </summary>
        event Action<double, double> OnMenuButtonPressed;

        /// <summary>
        /// Fires on short tap or single-click input cycles, providing the execution timestamps.
        /// </summary>
        event Action<double> OnTapped;

        /// <summary>
        /// Fires when a window handle changes its width or height bounding box size.
        /// </summary>
        event Action<double, double> OnResizeed;

        /// <summary>
        /// Fires when a window handle crosses over to a different screen space boundary.
        /// </summary>
        event Action<IntPtr> OnMonitorChanged;
        #endregion
    }
}


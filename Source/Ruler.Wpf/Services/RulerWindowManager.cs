using Microsoft.Extensions.DependencyInjection;

using Ruler.Contracts.Models;
using Ruler.Contracts.Services.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace Ruler.Wpf.Services
{
    public class RulerWindowManager : IRulerWindowManager
    {
        private readonly IServiceProvider _serviceProvider;

        public RulerWindowManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IntPtr CreateWindow(IRulerInfo info, Action onClosed)
        {
            // Resolve WPF dependencies locally inside the UI boundaries
            RulerViewModel viewModel = _serviceProvider.GetRequiredService<RulerViewModel>();
            MainWindow window = _serviceProvider.GetRequiredService<MainWindow>();

            viewModel.SetInitialState(info); // Push state profile into view model instance
            window.InitializeViewModel(viewModel); // Bind window view context explicitly

            IntPtr hwnd = IntPtr.Zero;

            // Hook window initialization to capture the native handle securely
            window.SourceInitialized += (s, e) =>
            {
                hwnd = new WindowInteropHelper(window).Handle; // Initialize viewmodel tracking handle
                viewModel.InitializeHwnd(hwnd);
            };

            window.Closed += (s, e) =>
            {
                onClosed?.Invoke(); // Execute abstract tracking callback on close
            };

            window.Show();

            // Fallback safety guard if handle allocation needs forced instantiation frames
            if (hwnd == IntPtr.Zero)
            {
                hwnd = new WindowInteropHelper(window).EnsureHandle();
                viewModel.InitializeHwnd(hwnd);
            }

            return hwnd;
        }

        public void ShutdownApplication()
        {
            // Terminate thread loops cleanly inside the native presentation stack
            Application.Current?.Shutdown();
        }
    }
}

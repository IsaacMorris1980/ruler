using Microsoft.Extensions.DependencyInjection;

using Ruler.Contracts.Services.OS;
using Ruler.Shared;

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Interop;


namespace Ruler.Wpf.Services
{
    public class AppManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILoggingService<AppManager> _loggingService;
        private readonly ISavingService _persistence;
        private readonly IEnvironmentService _environmentService;
        private readonly IWindowPlacementService _windowPlacementService;

        // Use the decoupled interface contract list type
        private readonly List<IRulerInfo> _activeRulers = new List<IRulerInfo>();
        private readonly List<Window> _activeWindows = new List<Window>();

        // Included missing dependencies and fixed the ServiceProvider assignment block
        public AppManager(
            IServiceProvider serviceProvider,
            ILoggingService<AppManager> logger,
            ISavingService strategy,
            IEnvironmentService environmentService,
            IWindowPlacementService windowPlacementService)
        {
            _serviceProvider = serviceProvider;
            _loggingService = logger;
            _persistence = strategy;
            _environmentService = environmentService;
            _windowPlacementService = windowPlacementService;
        }

        public void Start(string[] args)
        {
            try
            {
                // Pull data back using the interface definition contract
                List<IRulerInfo> savedRulers = _persistence.Load();

                if (savedRulers != null && savedRulers.Count > 0)
                {
                    foreach (IRulerInfo info in savedRulers)
                    {
                        CreateRuler(info);
                    }
                }
                else
                {
                    // Fallback to instantiate a default ruler instance if no profile exists
                    // CreateRuler(new RulerInfo { Width = 400, Height = 50 });
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Failed to initialize saved application profiles: " + ex.Message);
            }
        }

        public void CreateRuler(IRulerInfo info)
        {
            if (info == null)
            {
                _loggingService.LogError("Cannot create ruler: IRulerInfo profile parameter is null.");
                return;
            }

            // Resolve dependencies cleanly via DI Container
            RulerViewModel viewModel = _serviceProvider.GetRequiredService<RulerViewModel>();
            MainWindow window = _serviceProvider.GetRequiredService<MainWindow>();

            // Push state profile into view model instance
            viewModel.SetInitialState(info);
            window.InitializeViewModel(viewModel);

            _activeWindows.Add(window);
            _activeRulers.Add(info);

            // Hook window initialization to register native subclassing hooks safely
            window.SourceInitialized += (s, e) =>
            {
                IntPtr hwnd = new WindowInteropHelper(window).Handle;

                // 1. Initialize viewmodel tracking handle
                viewModel.InitializeHwnd(hwnd);

                // 2. Registers unmanaged subclass window procedures safely via comctl32.dll
                _environmentService.RegisterWindow(hwnd);

                // 3. Ensure the window handle sits visibility checked inside monitor areas
                _windowPlacementService.EnsureVisibility(hwnd);
            };

            window.Closed += (s, e) =>
            {
                _activeWindows.Remove(window);
                _activeRulers.Remove(info);

                // Unregister low-level unmanaged pointers on window destruction
                IntPtr hwnd = new WindowInteropHelper(window).Handle;
                _environmentService.UnregisterWindow(hwnd);

                if (_activeWindows.Count == 0)
                {
                    Shutdown();
                }
            };

            window.Show();
        }

        /// <summary>
        /// Call this when the app is closing or when a "Save" button is clicked
        /// </summary>
        public void Shutdown()
        {
            try
            {
                // Unhook active window subclass routing chains to clear pipeline overhead
                _environmentService.UnregisterAllWindows();

                // Save out collection using interface state definitions
                _persistence.Save(_activeRulers);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error occurred while executing shutdown saving state persistence: " + ex.Message);
            }
        }
    }
}

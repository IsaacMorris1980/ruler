using Microsoft.Extensions.DependencyInjection;

using Ruler.Wpf.Models;
using Ruler.Wpf.Services.Persistence;
using Ruler.Wpf.Services.Persistence.Strategy;
using Ruler.Wpf.ViewModels;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Ruler.Wpf.Services
{
    public class AppManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly List<RulerInfo> _activeRulers = new();
        private readonly List<Window> _activeWindows = new();   
        private readonly ILoggingService<AppManager> _loggingService;
        private readonly ISavingService _persistence;
        private readonly MonitorManager _monitorManager;
        private readonly DataServices _dataService;
        // The PersistenceStrategy (SavingService) is injected via the constructor
        public AppManager(ILoggingService<AppManager> logger,ISavingService strategy,MonitorManager manager)
        {
            _monitorManager = manager;
            _loggingService = logger;
            _persistence = strategy;
        }
        public void Start(string[] args)
        {
             _monitorManager.;
            _monitorManager.SynchronizeProfiles(_monitorManager._monitorProfiles);
           
                var savedRulers = _persistence.Load<RulerInfo>();
                foreach (var info in savedRulers)
                {
                    CreateRuler(info);
                }
            
        }
        public void CreateRuler(RulerInfo info)
        {
            if (info == null)
            {
                _loggingService.LogError("Cannot create ruler: RulerInfo is null.");
                return;
            }
            // Manually create the VM and Window pair
            var viewModel = _serviceProvider.GetRequiredService<RulerViewModel>();
            viewModel.SetInitialState(info); // Pass the RulerInfo to the VM for setup
            var window = _serviceProvider.GetRequiredService<MainWindow>();
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
            viewModel.InitializeHwnd(hwnd);
            window.InitializeViewModel(viewModel);
            _activeWindows.Add(window);

            window.Closed += (s, e) =>
            {
                _activeWindows.Remove(window);
                Shutdown();
            };  
            window.Show();
        }
        /// <summary>
        /// Call this when the app is closing or when a "Save" button is clicked
        /// </summary>
        public void Shutdown()
        {
           
            _persistence.Save(_activeRulers);
        }
    }
}

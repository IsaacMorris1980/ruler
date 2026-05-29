using Ruler.Contracts.Models;
using Ruler.Contracts.Services.OS;
using Ruler.Contracts.Services.Persistance;
using Ruler.Contracts.Services.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Services
{
    public class AppManager
    {
        private readonly ILoggingService<AppManager> _loggingService;
        private readonly ISavingService _persistence; // From user summary: handles C# state saving orchestration
        private readonly IEnvironmentService _environmentService;
        private readonly IWindowPlacementService _windowPlacementService;
        private readonly IRulerWindowManager _windowManager; // Abstracted UI gatekeeper

        private readonly List<IRulerInfo> _activeRulers = new List<IRulerInfo>();
        private readonly List<IntPtr> _activeHwnds = new List<IntPtr>();

        public AppManager(
            ILoggingService<AppManager> logger,
            ISavingService strategy, // Utilizing your C# SavingService configuration
            IEnvironmentService environmentService,
            IWindowPlacementService windowPlacementService,
            IRulerWindowManager windowManager)
        {
            _loggingService = logger;
            _persistence = strategy;
            _environmentService = environmentService;
            _windowPlacementService = windowPlacementService;
            _windowManager = windowManager;
        }

        public void Start(string[] args)
        {
            try
            {
                List<IRulerInfo> savedRulers = _persistence.Load(); // Pull layout states using the contract profile list

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

            _activeRulers.Add(info);

            // Ask the window manager interface to draw the UI and hand back its native HWND thread anchor
            IntPtr hwnd = _windowManager.CreateWindow(info, () => OnRulerWindowClosed(info));

            if (hwnd != IntPtr.Zero)
            {
                _activeHwnds.Add(hwnd);

                // Safe cross-cutting native registration triggers
                _environmentService.RegisterWindow(hwnd);
                _windowPlacementService.EnsureVisibility(hwnd);
            }
        }

        private void OnRulerWindowClosed(IRulerInfo info)
        {
            _activeRulers.Remove(info);

            // Clean down tracking states safely
            if (_activeRulers.Count == 0)
            {
                Shutdown();
            }
        }

        public void Shutdown()
        {
            try
            {
                // Unregister unmanaged hooks using standard platform services
                _environmentService.UnregisterAllWindows();

                // Persist data context profiles back using your implementation rules
                _persistence.Save(_activeRulers);

                // Signal the UI shell to close application frames
                _windowManager.ShutdownApplication();
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error occurred while executing shutdown saving state persistence: " + ex.Message);
            }
        }
    }
}

using Ruler.Shared.Interfaces;
using Ruler.Shared.Models;
using Ruler.Wpf.ViewModels;
using Ruler.Wpf.Windows;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Ruler.Wpf.Controllers
{
    public class AppController
    {
        private readonly IRulerRegistry _registry;
        private readonly IPersistanceService _settings;
        private readonly IRulerFactory _rulerFactory;
        private readonly IMainFormFactory _mainFormFactory;
        private readonly Func<RulerInfo, MainViewModel> _viewModelFactory;

        public AppController(
            IRulerRegistry registry,
            IPersistanceService settings,
            IRulerFactory factory,
            IMainFormFactory mainFormFactory,
            Func<RulerInfo, MainViewModel> viewModelFactory)
        {
            _registry = registry;
            _settings = settings;
            _rulerFactory = factory;
            _mainFormFactory = mainFormFactory;
            _viewModelFactory = viewModelFactory;
        }

        /// <summary>
        /// Entry point for the application logic.
        /// </summary>
        public void Run(string[] args)
        {
            // 1. Handle command-line arguments
            if (args != null && args.Length > 0)
            {
                var settings = _rulerFactory.CreateFromArguments(args);
                ShowRuler(settings);
            }
            else
            {
                // 2. Load existing state ONLY if no arguments were provided
                LoadAll();
            }
        }

        private void LoadAll()
        {
            var rulers = _settings.LoadAll();
            if (rulers != null && rulers.Any())
            {
                foreach (var info in rulers)
                {
                    ShowRuler(info);
                }
            }
            else
            {
                ShowRuler(_rulerFactory.CreateDefault());
            }
        }

        private void ShowRuler(RulerInfo info)
        {
            var viewModel = _viewModelFactory(info);
            var rulerWindow = new MainWindow(viewModel);

            // Handle duplication cleanly
            viewModel.DuplicateRequested += (sender, newInfo) =>
            {
                var copy = new RulerInfo();
                // Copy values logic...
                ShowRuler(copy);
            };

            rulerWindow.Show();
        }

        private void OnWindowClosed(object sender, EventArgs e)
        {
            // Cleanup logic
            if (sender is Window window && window.DataContext is IRulerViewModel rulerVm)
            {
                // Save settings using the shared interface property
                _settings.Update(rulerVm.RulerData);

                // Unregister the ViewModel instance from the registry
                _registry.Unregister(rulerVm);
            }

            // 2. Check if any rulers are still active using your registry's collection
            var active = _registry.ActiveRulers;

            // 3. Shutdown application if no active rulers remain
            if (!active.Any())
            {
                Application.Current.Shutdown();
            }
        }
    }
}

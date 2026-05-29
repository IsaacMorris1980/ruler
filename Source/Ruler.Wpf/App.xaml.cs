using Ruler.Shared.Factories;
using Ruler.Shared.Models;
using Ruler.Shared.Services;
using Ruler.Wpf.Infrastructure;
using Ruler.Wpf.Properties;
using Ruler.Wpf.ViewModels;
using Ruler.Wpf.Views;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Ruler.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Pull the raw JSON string out of the WPF application settings configuration object
            string json = Ruler.Wpf.Properties.Settings.Default.RulerCollection;

            // 2. Deserialize into core data models using your shared service
            List<RulerInfo> loadedRulers = SettingsService.DeserializeRulers(json);

            if (loadedRulers == null || !System.Linq.Enumerable.Any(loadedRulers))
            {
                loadedRulers = new List<RulerInfo>{ RulerFactory.CreateDefault() };
            }

            // 3. Populate the central tracking state bucket
            RulerSessionManager.Initialize(loadedRulers);

            // 4. Instantiate UI views for the models
            foreach (var model in loadedRulers)
            {
                OpenNewRulerWindow(model);
            }
        }

        public static void OpenNewRulerWindow(RulerInfo model)
        {
            // Register model to track it if spawned during runtime (e.g. Duplication)
            RulerSessionManager.RegisterNewRuler(model);

            var viewModel = new ViewModels.RulerViewModel(model);
            var view = new RulerWindow { DataContext = viewModel };
            view.Show();
        }

        public static void OpenRulerInstance(RulerInfo model)
        {
            var viewModel = new RulerViewModel(model);
            var view = new RulerWindow { DataContext = viewModel };
            view.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Final verification pipeline cleanup before application memory is released
            RulerSessionManager.PersistToSettings();
            base.OnExit(e);
        }
    }
}

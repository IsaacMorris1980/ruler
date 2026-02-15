using Metalama.Framework.Services;

using Microsoft.Extensions.DependencyInjection;

using Ruler.Wpf.Common;
using Ruler.Wpf.Models;
using Ruler.Wpf.Services;
using Ruler.Wpf.Services.Persistence;
using Ruler.Wpf.Services.Persistence.Strategy;
using Ruler.Wpf.ViewModels;

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
        public IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Setup Logging and Initial Load Strategy
            var log = new LoggingService();
            var settingsStrategy = new SettingsPersistenceStrategy(log);

            // 2. Determine initial state (Command line or Saved settings)
            RulerInfo initialInfo;
            if (e.Args.Length > 0)
            {
                initialInfo = CommandLineRulerFactory.CovertToRulerInfo(e.Args);
            }
            else
            {
                initialInfo = settingsStrategy.Load();
            }

            // 3. Configure Dependency Injection
            ServiceCollection serviceCollection = new ServiceCollection();

            // Register instances and services
            serviceCollection.AddSingleton(initialInfo);
            serviceCollection.AddSingleton<IEnvironmentService, EnvironmentService>();
            serviceCollection.AddSingleton<ILoggingService>(log);
            serviceCollection.AddSingleton<SettingsPersistenceStrategy>(settingsStrategy);
            serviceCollection.AddSingleton<SingleRulerPersistenceService>();
            serviceCollection.AddSingleton<IDialogService, DialogService>();

            // Register ViewModels and Windows
            serviceCollection.AddTransient<RulerViewModel>();
            serviceCollection.AddTransient<MainWindow>();

            ServiceProvider = serviceCollection.BuildServiceProvider();

            // 4. Run Startup Logic
            ExecuteStartupLogic();
        }

        private void ExecuteStartupLogic()
        {
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            var dialogService = ServiceProvider.GetRequiredService<IDialogService>();

            // Register the window with the dialog service if it manages multiple instances
            dialogService.AddRuler(mainWindow);

            mainWindow.Show();
        }

    }
}


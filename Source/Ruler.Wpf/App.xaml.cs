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

        
       

            // 3. Configure Dependency Injection
            ServiceCollection serviceCollection = new ServiceCollection();
            // Register instances and service
            serviceCollection.AddSingleton<IEnvironmentService, EnvironmentService>();
            serviceCollection.AddSingleton(typeof(ILoggingService<>),typeof(DebugLoggingService<>));
            serviceCollection.AddSingleton<IPersistenceStrategy, SavingService>();
            serviceCollection.AddSingleton<IDialogService, DialogService>();
            serviceCollection.AddSingleton<SavingService>();
            serviceCollection.AddSingleton<AppManager>();
            serviceCollection.AddSingleton<MonitorManager>();
            // Register ViewModels and Windows
            serviceCollection.AddTransient<RulerViewModel>();
            serviceCollection.AddTransient<MainWindow>();
            ServiceProvider = serviceCollection.BuildServiceProvider();
            // 4. Run Startup Logic
          AppManager appManager = ServiceProvider.GetRequiredService<AppManager>();
          
        }

      

    }
}


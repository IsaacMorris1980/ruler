using Ruler.Shared.Factories;
using Ruler.Shared.Models;
using Ruler.Wpf.Infrastructure;
using Ruler.Wpf.ViewModels;
using Ruler.Wpf.Views;
using System.Reflection;    
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;

public partial class App : Application
{
    private readonly RulerSessionManager _sessionManager;

    // Thread-local guard flag to prevent the resolver from calling itself infinitely
    [ThreadStatic]
    private static bool _isResolving;

    static App()
    {
        AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
        {
            // 1. Guard against recursive infinite loops
            if (_isResolving) return null;

            try
            {
                _isResolving = true;

                string assemblyName = new AssemblyName(args.Name).Name;

                // 2. Ignore native system, manifest resources, or localization satellite assemblies
                if (assemblyName.StartsWith("System") ||
                    assemblyName.StartsWith("mscorlib") ||
                    assemblyName.EndsWith(".resources"))
                {
                    return null;
                }

                var currentAssembly = Assembly.GetExecutingAssembly();

                // Look through the embedded Costura payload streams
                //string resourceName = currentAssembly.GetManifestResourceNames()
                //    .FirstOrDefault(r => r.IndexOf(assemblyName, StringComparison.OrdinalIgnoreCase) >= 0);

                //if (string.IsNullOrEmpty(resourceName)) return null;

                //using (var stream = currentAssembly.GetManifestResourceStream(resourceName))
                //{
                //    if (stream == null) return null;
                //    byte[] assemblyData = new byte[stream.Length];
                //    stream.Read(assemblyData, 0, assemblyData.Length);
                //    return Assembly.Load(assemblyData);
                //}
                return null;
            }
            finally
            {
                // Always clear the flag when leaving the method context
                _isResolving = false;
            }
        };
    }
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        RulerInfo testModel = new RulerInfo { Width = 500, Height = 80, Left = 100, Top = 100 };

        RulerWindow window = new RulerWindow
        {
            DataContext = new RulerViewModel(testModel),
            Width = 500,
            Height = 80
        };
        window.Show();

        //List<RulerInfo> savedSession = _sessionManager.LoadSession().ToList();

        //if (savedSession != null && savedSession.Count > 0)
        //{
        //    foreach (RulerInfo blueprint in savedSession)
        //    {
        //        LaunchRulerWindow(blueprint);
        //    }
        //}
        //else
        //{
        //    LaunchRulerWindow(RulerFactory.CreateDefault());
        //}
    }

    private void LaunchRulerWindow(RulerInfo blueprint)
    {
        RulerViewModel viewModel = new RulerViewModel
        {
            Width = blueprint.Width > 0 ? blueprint.Width : 500,
            Height = blueprint.Height > 0 ? blueprint.Height : 80,
            Left = blueprint.Left,
            Top = blueprint.Top,
            Opacity = blueprint.Opacity,
            SaveType = blueprint.SaveType
        };

        RulerWindow window = new RulerWindow
        {
            DataContext = viewModel,
            Width = viewModel.Width,
            Height = viewModel.Height,
            Left = viewModel.Left,
            Top = viewModel.Top,
            Opacity = viewModel.Opacity
        };
        window.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);
    }
}
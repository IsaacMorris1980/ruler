using Ruler.Contracts.Models;
using Ruler.Contracts.Services.UI;
using Ruler.Shared;
using Ruler.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows;
using Ruler.Wpf.Views.Dialogs;

using Ruler.Contracts.Strategies;

namespace Ruler.Wpf.Services
{
    public class WpfDialogService : IDialogService
        {
            private readonly IEnumerable<IUnitStrategy> _allStrategies;

            /// <summary>
            /// Initializes the dialog service and injects all registered conversion strategies.
            /// </summary>
            public WpfDialogService(IEnumerable<IUnitStrategy> allStrategies)
            {
                // Defensive check: Crash early during startup if DI container setup is corrupted
                _allStrategies = allStrategies ?? throw new ArgumentNullException(nameof(allStrategies));
            }

            /// <summary>
            /// Displays the modal dialog to manually override the ruler's dimensions.
            /// </summary>
            public DialogSizeResult ShowSetSizeDialog(double currentWidth, double currentHeight, IUnitStrategy currentActiveStrategy)
            {
                // 1. Build the shared presentation view model instance
                var viewModel = new SetSizeViewModel(currentWidth, currentHeight, currentActiveStrategy, _allStrategies);

                // 2. Instantiate the physical WPF Window view asset
                var window = new SetSizeWindow
                {
                    DataContext = viewModel,
                    Owner = Application.Current.MainWindow // Center it relatively to the parent window frame
                };

                // 3. ShowDialog halts synchronous execution lines until the user saves or exits
                bool? dialogResult = window.ShowDialog();

                if (dialogResult == true)
                {
                    // Success: Pass the user inputs and targeted strategy enum token back
                    return new DialogSizeResult(viewModel.WidthInput, viewModel.HeightInput, viewModel.SelectedStrategy.UnitType);
                }

                // Aborted/Closed: Return original parameters safely unchanged
                return new DialogSizeResult(currentWidth, currentHeight, currentActiveStrategy.UnitType);
            }

            /// <summary>
            /// Displays the borderless interactive visual calibration wizard.
            /// </summary>
            public CalibrationResult? ShowCalibrationDialog(double defaultSliderValue)
            {
                // 1. Build the tracking calibration presentation state
                var viewModel = new CalibrationViewModel(defaultSliderValue);

                // 2. Build the borderless overlay window shell
                var window = new CalibrationWindow
                {
                    DataContext = viewModel,
                    Owner = Application.Current.MainWindow
                };

                // 3. Halt background execution context frames until submission completes
                bool? dialogResult = window.ShowDialog();

                if (dialogResult == true)
                {
                    // Success: Package up the computed raw pixels-per-millimeter scale
                    return new CalibrationResult(viewModel.CalculatedPixelsPerMm);
                }

                // Aborted/Canceled: Return null to safely reject mutations to application settings
                return null;
            }

            /// <summary>
            /// Displays a standard platform message alert overlay box.
            /// </summary>
            public void ShowAboutDialog(string message)
            {
                MessageBox.Show(
                    Application.Current.MainWindow ?? throw new InvalidOperationException("Main application frame is uninitialized."),
                    message,
                    "About Ruler",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
    }
}
using Microsoft.Extensions.DependencyInjection;
using Ruler.Wpf.Models;
using Ruler.Wpf.Services.Persistence;
using Ruler.Wpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Ruler.Wpf.Services
{
    /// <summary>
    /// An implementation of IDialogService that shows the SetSizeWindow.
    /// </summary>
    public class DialogService : IDialogService
    {
        private readonly ILoggingService<DialogService> _loggingService;
        public DialogService(ILoggingService<DialogService> loggingService)
        {
            _loggingService = loggingService;
        }
        /// <summary>
        /// Shows the SetSizeWindow dialog.
        /// </summary>
        /// <returns>A Size object containing the new width and height if the user
        /// clicks OK; otherwise, returns null.</returns>
        public Size ShowSetSizeDialog(double width, double height)
        {
            // Create a new instance of the SetSizeWindow.
            var setSizeWindow = new SetSizeWindow(width, height);

            // Show the dialog and capture the result.
            bool? result = setSizeWindow.ShowDialog();

            // Check if the DialogResult was true (the user clicked the OK button).
            if (result == true)
            {
                // If the dialog was successful, return the new size from the window's property.
                return setSizeWindow.NewSize;
            }

            // If the dialog was cancelled, return null.
            return new Size(400, 200);
        }
        public void ShowAboutDialog(string message)
        {

            MessageBox.Show(message, "About Ruler", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

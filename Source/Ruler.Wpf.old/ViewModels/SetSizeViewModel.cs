using Ruler.Wpf.Infrastructure;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Ruler.Wpf.ViewModels
{
    public class SetSizeViewModel : ViewModelBase
    {
        #region Private Fields
        private double _targetWidth;
        private double _targetHeight;
        private string _errorMessage;
        private bool? _dialogResult;
        #endregion

        #region Events
        // Events that the View hooks into to cleanly tear down the window frame
        public event EventHandler CloseRequested;
        public event EventHandler CancelRequested;
        #endregion

        #region Bound Properties
        public double TargetWidth
        {
            get => _targetWidth;
            set
            {
                _targetWidth = value;
                OnPropertyChanged();
                ValidateDimensions();
            }
        }

        public double TargetHeight
        {
            get => _targetHeight;
            set
            {
                _targetHeight = value;
                OnPropertyChanged();
                ValidateDimensions();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }
        // 2. Define the public property that matches what the view is looking for
        public bool? DialogResult
        {
            get => _dialogResult;
            set
            {
                _dialogResult = value;
                OnPropertyChanged(); // Crucial: Tells the view-behind that the value changed
            }
        }
        #endregion

        #region Commands
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        #endregion

        #region Constructor
        public SetSizeViewModel(double currentWidth, double currentHeight)
        {
            // Set the inputs to mirror the ruler's exact active bounds upon launching
            TargetWidth = currentWidth;
            TargetHeight = currentHeight;

            // Wire up commands using your MVVM architecture's validation handler parameter
            SaveCommand = new RelayCommand(ExecuteSave, CanSave);
            CancelCommand = new RelayCommand(ExecuteCancel);
        }
        #endregion

        #region Command Methods
        private bool CanSave(object parameter)
        {
            // The Apply button will visually disable itself automatically if an error string exists
            return string.IsNullOrEmpty(ErrorMessage);
        }

        private void ExecuteSave(object parameter)
        {
            if (CanSave(parameter))
            {
                // Fire the event to tell the window's code-behind to set DialogResult = true
                CloseRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        private void ExecuteCancel(object parameter)
        {
            // Fire the event to tell the window's code-behind to set DialogResult = false
            CancelRequested?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region Validation Logic
        private void ValidateDimensions()
        {
            // Enforce safe boundaries so the UI window layout doesn't crash or disappear
            if (TargetWidth < 40)
            {
                ErrorMessage = "Width must be at least 40 pixels.";
            }
            else if (TargetHeight < 20)
            {
                ErrorMessage = "Height must be at least 20 pixels.";
            }
            else if (TargetWidth > 3840 || TargetHeight > 2160)
            {
                ErrorMessage = "Dimensions cannot exceed 4K display limits.";
            }
            else
            {
                ErrorMessage = string.Empty; // Clear errors when everything looks healthy
            }
        }
        #endregion
    }
}

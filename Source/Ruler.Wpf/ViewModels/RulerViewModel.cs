using Ruler.Wpf.Infrastructure;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Ruler.Shared.Models;
using Ruler.Shared.Factories;
using Ruler.Shared.Enums;

namespace Ruler.Wpf.ViewModels
{
    public class RulerViewModel : ViewModelBase
    {
        private readonly RulerInfo _model;

        public RulerInfo Model => _model;

        public RulerViewModel(RulerInfo model)
        {
            _model = model ?? RulerFactory.CreateDefault();

            // Wire commands
            ToggleTopMostCommand = new RelayCommand(_ => TopMost = !TopMost);
            ToggleLockCommand = new RelayCommand(_ => IsLocked = !IsLocked);
            ToggleOrientationCommand = new RelayCommand(_ => IsVertical = !IsVertical);
            ToggleTooltipCommand = new RelayCommand(_ => ShowToolTip = !ShowToolTip);
            ToggleGuidelineCommand = new RelayCommand(_ => IsGuidelineEnabled = !IsGuidelineEnabled);

            DuplicateCommand = new RelayCommand(_ => DuplicateRuler());
            SetSizeCommand = new RelayCommand(_ => OpenSizeDialog());
            ResetDefaultCommand = new RelayCommand(_ => ResetToDefault());
            ClearAllSavedCommand = new RelayCommand(_ => ClearAllSavedRulers());
            CloseCommand = new RelayCommand(win => (win as Window)?.Close());
            ExitCommand = new RelayCommand(_ => App.Current.Shutdown());

            InitializeOpacityOptions();
        }

        // Exposed properties mapped to the underlying model
        public double Width
        {
            get => _model.Width;
            set { if (_model.Width != (int)value) { _model.Width = (int)value; OnPropertyChanged(); } }
        }

        public double Height
        {
            get => _model.Height;
            set { if (_model.Height != (int)value) { _model.Height = (int)value; OnPropertyChanged(); } }
        }

        public bool TopMost
        {
            get => _model.TopMost;
            set { if (_model.TopMost != value) { _model.TopMost = value; OnPropertyChanged(); } }
        }

        public bool IsLocked
        {
            get => _model.IsLocked;
            set { if (_model.IsLocked != value) { _model.IsLocked = value; OnPropertyChanged(); } }
        }

        public bool IsVertical
        {
            get => _model.IsVertical;
            set
            {
                _model.ToggleOrientation();
                OnPropertyChanged();
                OnPropertyChanged(nameof(Width));
                OnPropertyChanged(nameof(Height));
            }
        }

        public bool ShowToolTip
        {
            get => _model.ShowToolTip;
            set { if (_model.ShowToolTip != value) { _model.ShowToolTip = value; OnPropertyChanged(); } }
        }

        public bool IsGuidelineEnabled
        {
            get => _model.IsGuidelineEnabled;
            set { if (_model.IsGuidelineEnabled != value) { _model.IsGuidelineEnabled = value; OnPropertyChanged(); } }
        }

        public double Opacity
        {
            get => _model.Opacity;
            set { if (Math.Abs(_model.Opacity - value) > 0.001) { _model.Opacity = value; OnPropertyChanged(); } }
        }

        public SaveTypes SaveType
        {
            get => _model.SaveType;
            set { if (_model.SaveType != value) { _model.SaveType = value; OnPropertyChanged(); } }
        }

        // Collection UI mapping arrays for standard sub-menus
        public ObservableCollection<double> OpacityLevels { get; private set; }
        public Array SaveTypeOptions => Enum.GetValues(typeof(SaveTypes));

        // Commands
        public ICommand ToggleTopMostCommand { get; }
        public ICommand ToggleLockCommand { get; }
        public ICommand ToggleOrientationCommand { get; }
        public ICommand ToggleTooltipCommand { get; }
        public ICommand ToggleGuidelineCommand { get; }
        public ICommand DuplicateCommand { get; }
        public ICommand SetSizeCommand { get; }
        public ICommand ResetDefaultCommand { get; }
        public ICommand ClearAllSavedCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand ExitCommand { get; }

        private void InitializeOpacityOptions()
        {
            OpacityLevels = new ObservableCollection<double>
            {
                0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1.0
            };
        }

        private void DuplicateRuler()
        {
            var newInfo = new RulerInfo();
            RulerFactory.CopyValues(_model, newInfo);
            var newVm = new RulerViewModel(newInfo);
            var newWindow = new RulerWindow { DataContext = newVm };
            newWindow.Show();
        }

        private void OpenSizeDialog()
        {
            var sizeVm = new SetSizeViewModel((int)Width, (int)Height);
            var sizeWin = new SetSizeWindow { DataContext = sizeVm };
            if (TopMost) sizeWin.TopMost = true;

            if (sizeWin.ShowDialog() == true)
            {
                Width = sizeVm.Width;
                Height = sizeVm.Height;
            }
        }

        private void ResetToDefault()
        {
            var defaults = RulerFactory.CreateDefault();
            RulerFactory.CopyValues(defaults, _model);
            OnPropertyChanged(string.Empty); // Refresh all bound fields
        }

        private void ClearAllSavedRulers()
        {
            var result = MessageBox.Show("Are you sure you want to clear all saved rulers? This cannot be undone.", "Confirm Clear", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                // Accessing the collection cleanup via your existing context wrappers
                foreach (var win in Application.Current.Windows.OfType<RulerWindow>())
                {
                    if (win.DataContext is RulerViewModel vm)
                    {
                        vm.SaveType = SaveTypes.none;
                    }
                }
                MessageBox.Show("All ruler configurations have been updated.");
            }
        }
    }
}

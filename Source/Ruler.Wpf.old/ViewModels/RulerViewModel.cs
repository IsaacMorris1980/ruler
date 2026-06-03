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
using Ruler .Wpf.Views;
using System.Windows.Controls;

namespace Ruler.Wpf.ViewModels
{
    public class RulerViewModel : ViewModelBase
    {
        #region Private Fields
        private double _width = 500;
        private double _height = 80;
        private double _left = 100;
        private double _top = 100;
        private double _opacity = 0.8;
        private bool _isVertical = false;
        private SaveTypes _saveType = SaveTypes.none;
        #endregion

        #region Bound Properties
        public double Width
        {
            get => _width;
            set { _width = value; OnPropertyChanged(); }
        }

        public double Height
        {
            get => _height;
            set { _height = value; OnPropertyChanged(); }
        }

        public double Left
        {
            get => _left;
            set { _left = value; OnPropertyChanged(); }
        }

        public double Top
        {
            get => _top;
            set { _top = value; OnPropertyChanged(); }
        }

        public bool IsVertical
        {
            get => _isVertical;
            set { _isVertical = value; OnPropertyChanged(); }
        }
        public double Opacity
        {
            get => _opacity;
            set { _opacity = value; OnPropertyChanged(); }
        }

        public SaveTypes SaveType
        {
            get => _saveType;
            set
            {
                _saveType = value;
                OnPropertyChanged();
                UpdateMenuCheckmarks(); // Keep visual checkmarks synchronized
            }
        }

        // The dynamic collection feeding into your ContextMenu DataTemplate
        public ObservableCollection<MenuBlueprint> MenuOptions { get; set; }
        #endregion

        #region Core Commands
        public ICommand ToggleOrientationCommand { get; }
        public ICommand SetSizeCommand { get; }
        public ICommand CheckForUpdatesCommand { get; }
        public ICommand ResetDefaultCommand { get; }
        public ICommand SetSaveTypeCommand { get; }
        public ICommand SetOpacityCommand { get; }


        #endregion

        #region Constructor
        public RulerViewModel()
        {
            // 1. Initialize Actions/Commands
            ToggleOrientationCommand = new RelayCommand(ExecuteToggleOrientation);
            SetSizeCommand = new RelayCommand(ExecuteSetSizeCommand);
            CheckForUpdatesCommand = new RelayCommand(ExecuteCheckForUpdates);
            ResetDefaultCommand = new RelayCommand(ExecuteResetDefault);

            // 2. Build the Visual Menu Collection Structure
            MenuOptions = new ObservableCollection<MenuBlueprint>
            {
                new MenuBlueprint("Toggle Orientation", ToggleOrientationCommand, "Space / Double-Click"),
                new MenuBlueprint("Set Custom Size...", SetSizeCommand, "Ctrl+S"),
                new MenuBlueprint("Check for Updates...", CheckForUpdatesCommand, "Ctrl+U"),
                new MenuBlueprint("Reset This Ruler", ResetDefaultCommand, "R"),
                
                // Add a visual menu separator line marker
                new MenuBlueprint(string.Empty, null) { IsSeparator = true }, 
                
                // Layout Save States sub-menu tree branching
                new MenuBlueprint("Save Rules Setup", null)
                {
                    SubItems = new ObservableCollection<MenuBlueprint>()
                    {
                        new MenuBlueprint("Save Type: None", new RelayCommand((p) => SaveType = SaveTypes.none), "N") { IsCheckable = true },
                        new MenuBlueprint("Save Type: Location", new RelayCommand((p) => SaveType = SaveTypes.location), "L") { IsCheckable = true },
                        new MenuBlueprint("Save Type: Size", new RelayCommand((p) => SaveType = SaveTypes.size), "Alt+S") { IsCheckable = true },
                        new MenuBlueprint("Save Type: All Parameters", new RelayCommand((p) => SaveType = SaveTypes.all), "A") { IsCheckable = true }
                    }
                }
            };

            // Force initial checkmark alignment mapping to SaveTypes.None
            UpdateMenuCheckmarks();
        }
        public RulerViewModel(RulerInfo model)
            {
            Width = model.Width;
            Height = model.Height;
            Left = model.Left;
            Top = model.Top;
            IsVertical = model.IsVertical;
            Opacity = model.Opacity;
            SaveType = model.SaveType;
        }
        #endregion

        #region Command Execution Methods
        private void ExecuteToggleOrientation(object parameter)
        {
            // Direct value flip swaps spatial bounds natively
            double oldWidth = Width;
            double oldHeight = Height;

            Width = oldHeight;
            Height = oldWidth;

            IsVertical = !IsVertical;
        }

        private void ExecuteSetSizeCommand(object parameter)
        {
            var dialogVm = new SetSizeViewModel(Width, Height);

            // Build view framework directly targeting the operating system window thread pool
            var dialogWindow = new Views.SetSizeWindow(dialogVm)
            {
                Owner = Application.Current?.MainWindow
            };

            if (dialogWindow.ShowDialog() == true)
            {
                Width = dialogVm.TargetWidth;
                Height = dialogVm.TargetHeight;
            }
        }

        private void ExecuteCheckForUpdates(object parameter)
        {
            // Your update manifest pipeline logic hook goes here
        }

        private void ExecuteResetDefault(object parameter)
        {
           
        }
        #endregion

        #region Helper Routines
        private void UpdateMenuCheckmarks()
        {
            // Safety guard: trace collection layout structure manually
            if (MenuOptions == null || MenuOptions.Count < 6) return;

            // Find the sub-menu container (index 5)
            var saveSubMenu = MenuOptions[5];
            if (saveSubMenu?.SubItems == null || saveSubMenu.SubItems.Count != 4) return;

            // Map UI boolean states cleanly based on the active Enum parameter state
            saveSubMenu.SubItems[0].IsChecked = (SaveType == SaveTypes.none);
            saveSubMenu.SubItems[1].IsChecked = (SaveType == SaveTypes.location);
            saveSubMenu.SubItems[2].IsChecked = (SaveType == SaveTypes.size);
            saveSubMenu.SubItems[3].IsChecked = (SaveType == SaveTypes.all);
        }
        public ContextMenu GenerateMenu()
        {
            var contextMenu = new ContextMenu();
            foreach (var item in MenuOptions)
            {
                if (item.IsSeparator)
                {
                    contextMenu.Items.Add(new Separator());
                    continue;
                }
                var menuItem = new MenuItem
                {
                    Header = item.Header,
                    Command = item.Command,
                    InputGestureText = item.InputGestureText,
                    IsCheckable = item.IsCheckable,
                    IsChecked = item.IsChecked
                };
                // Recursively add sub-menu items if they exist
                if (item.SubItems != null && item.SubItems.Count > 0)
                {
                    foreach (var subItem in item.SubItems)
                    {
                        var subMenuItem = new MenuItem
                        {
                            Header = subItem.Header,
                            Command = subItem.Command,
                            InputGestureText = subItem.InputGestureText,
                            IsCheckable = subItem.IsCheckable,
                            IsChecked = subItem.IsChecked
                        };
                        menuItem.Items.Add(subMenuItem);
                    }
                }
                contextMenu.Items.Add(menuItem);
            }
            return contextMenu;
        }
        #endregion
    }
}


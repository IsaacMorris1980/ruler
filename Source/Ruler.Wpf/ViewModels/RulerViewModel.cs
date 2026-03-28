using Microsoft.Extensions.DependencyInjection;

using Ruler.Wpf.Common;
using Ruler.Wpf.Enums;
using Ruler.Wpf.Models;
using Ruler.Wpf.Services;
using Ruler.Wpf.Services.Persistence;
using Ruler.Wpf.Services.Persistence.Strategy;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Ruler.Wpf.ViewModels
{
    // The core ViewModel class for the Ruler application
    public class RulerViewModel : ViewModelBase
    {
        #region Fields
        // The RulerInfo class would be our Model
        private RulerInfo _rulerInfo;
        private ICommand _toggleLockCommand;
        private ICommand _exitCommand;
        private ICommand _toggleVerticalCommand;
        private ICommand _toggleTopMostCommand;
        private ICommand _toggleToolTipCommand;
        private ICommand _setOpacityCommand;
        private ICommand _setSaveTypeCommand;
        private ICommand _showSetSizeFormCommand;
        private ICommand _showAboutCommand;
        private ICommand _duplicateCommand;
        private ICommand _resetToDefaultCommand;
        private ICommand _manualScaleCommand;
        private ICommand _setScaleCommand;
        private ICommand _setUnitCommand;
        private ICommand _enableMagnifierCommand;
        private bool _isInitialized = false;
        private IDialogService _dialogService;
        private ISavingService _persistenceService;   
        private IEnvironmentService _environmentService;
            private ILoggingService<RulerViewModel> _loggingService;
        private IServiceProvider _serviceProvider;
        private IntPtr _currentMonitorHandle = IntPtr.Zero; 
        // State variables for mouse interaction
        private bool _isLoadingState;
        public Guid ViewModelGuid => _rulerInfo.Id;
        private double _systemDpiScale = 1.0;
        private double _currentPhysicalScale = 1.0;
        private ILoggingService<RulerViewModel> _logger;
        private double _currentEffectiveScale;
        private double ManualScaleValue;

        #region Constructor
        public RulerViewModel(IServiceProvider service)
        {
            _serviceProvider = service; // This constructor is for design-time only and should not be used at runtime
            _dialogService = service.GetRequiredService<IDialogService>();
            _persistenceService = service.GetRequiredService<ISavingService>();
            _loggingService = service.GetRequiredService<ILoggingService<RulerViewModel>>();
            _environmentService = service.GetRequiredService<IEnvironmentService>(); 
            InitializeCommands();
            InitializeUnits();
            InitializeOpacities();
            InitializeScales();
            InitializeSaveTypes();
            InitializeMagnifyScales();
            _environmentService.OnWindowMoved += UpdateLocation;
            _environmentService.OnDpiChanged += UpdateSystemScale;

        }
        public void InitializeHwnd(IntPtr hwnd)
        {
            _currentMonitorHandle = hwnd;
        }
        /// <summary>
        /// Replaces OrientationToAngleConverter
        /// </summary>
        public double RulerRotationAngle => IsVertical ? -90 : 0;

        /// <summary>
        /// Replaces BooleanToVisibilityConverter for Magnifier
        /// </summary>
        public Visibility MagnifierVisibility => IsMagnifierEnabled ? Visibility.Visible : Visibility.Collapsed;


       
        #endregion
        #region Properties
        public bool IsGuidelineLocked
        {
            get => _rulerInfo.Guideline.IsLocked;
            set
            {
                if (_rulerInfo.Guideline.IsLocked == value) return;
                _rulerInfo.SuppressNotifications = true;
                try
                {
                    SetProperty(_rulerInfo.Guideline.IsLocked, value, v => _rulerInfo.Guideline.IsLocked = v, nameof(IsGuidelineLocked));
                }
                finally
                {
                    _rulerInfo.SuppressNotifications = false;
                }
            }
        }
        public bool IsGuideLineVisible
        {
            get => _rulerInfo.Guideline.IsVisible;
            set
            {   if (_rulerInfo.Guideline.IsVisible == value) return;
                _rulerInfo.SuppressNotifications = true;
                try
                {
                    SetProperty(_rulerInfo.Guideline.IsVisible, value, v => _rulerInfo.Guideline.IsVisible = v, nameof(IsGuideLineVisible));
                }
                finally
                {
                    _rulerInfo.SuppressNotifications = false;
                }
            }
        }
        public double GuideLinePosition
        {
            get => _rulerInfo.Guideline.Position;
            set
            {   if (Math.Abs(_rulerInfo.Guideline.Position - value) < 0.001) return;
                _rulerInfo.SuppressNotifications = true;
                try
                {
                   SetProperty(_rulerInfo.Guideline.Position, value, v => _rulerInfo.Guideline.Position = v, nameof(GuideLinePosition));
                }
                finally
                {
                    _rulerInfo.SuppressNotifications = false;
                }
            }
        }
        public ResizeMode WindowResizeMode
        {
            get => IsLocked ? ResizeMode.NoResize : ResizeMode.CanResize;
        }
        public Color GuideLineColor
        {
            get => _rulerInfo.Guideline.Color;
            set
                {
                if (_rulerInfo.Guideline.Color == value) return;
                _rulerInfo.SuppressNotifications = true;
                try
                {
                    SetProperty(_rulerInfo.Guideline.Color, value, v => _rulerInfo.Guideline.Color = v, nameof(GuideLineColor));
                }
                finally
                {
                    _rulerInfo.SuppressNotifications = false;
                }
            }
        }
      
        public MeasurementUnit CurrentUnit
        {
            get => _rulerInfo.CurrentUnit;
            set
            {
                if (_rulerInfo.CurrentUnit == value) return;
                _rulerInfo.SuppressNotifications = true;
                try
                {
                   SetProperty(_rulerInfo.CurrentUnit, value, v => _rulerInfo.CurrentUnit = v, nameof(CurrentUnit));
                    UpdateUnitSelection(value);
                }
                finally
                {
                    _rulerInfo.SuppressNotifications = false;
                }

            }
        }
        // Property for the ruler's width, with change notification
        public double Width
        {
            get => _rulerInfo.Width;
            set
            {
                if (Math.Abs(_rulerInfo.Width - value) < 0.001) return;
                _rulerInfo.SuppressNotifications = true;
                try
                {
                    SetProperty(_rulerInfo.Width, value, v => _rulerInfo.Width = v, nameof(Width));
                }
                finally
                {
                    _rulerInfo.SuppressNotifications = false;
                }
            }
        }
        // Property for the ruler's height, with change notification
        public double Height
        {
            get => _rulerInfo.Height;
            set
            {
                if (Math.Abs(_rulerInfo.Height - value) < 0.001) return;
                _rulerInfo.SuppressNotifications = true;
                try
                {
                   SetProperty(_rulerInfo.Height, value, v => _rulerInfo.Height = v, nameof(Height));
                }
                finally
                {
                    _rulerInfo.SuppressNotifications = false;
                }
            }
        }
        public double Left
        {
            get => _rulerInfo.Left;
            set
            {
               if (Math.Abs(_rulerInfo.Left - value) < 0.001) return;
                _rulerInfo.SuppressNotifications = true;
                try
                {
                    SetProperty(_rulerInfo.Left, value, v => _rulerInfo.Left = v, nameof(Left));
                }
                finally
                {
                    _rulerInfo.SuppressNotifications = false;
                }
            }
        }
    
        public RulerInfo RulerInformation
        {
            get => _rulerInfo;
            set
            {
                if (_rulerInfo == value) return;
                if (value == null)
                {
                    ArgumentNullException a = new ArgumentNullException(nameof(RulerInfo), "RulerInfo is null.");
                    _loggingService.LogError("Failed to initialize RulerViewModel with null RulerInfo.", a);
                    MessageBox.Show("An error occurred while initializing the ruler. The application will now close.", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown();
                    return;
                }
                _rulerInfo = value;
            }
        }
        public double Top
        {
            get => _rulerInfo.Top;
            set 
            {
                if (Math.Abs(_rulerInfo.Top - value) < 0.001) return;
                _rulerInfo.SuppressNotifications = true;
                try
                {
                   SetProperty(_rulerInfo.Top, value, v=> _rulerInfo.Top = v,nameof(Top));
                }
                finally
                {
                    _rulerInfo.SuppressNotifications = false;
                }
            } 
        }
       public bool IsLocked
        {
            get => _rulerInfo.IsLocked;
            set 
            {
                if (_rulerInfo.IsLocked==value) return;
                _rulerInfo.SuppressNotifications = true;
                try
                {
                    SetProperty(_rulerInfo.IsLocked,value, v=> _rulerInfo.IsLocked = v,nameof(IsLocked));
                    OnPropertyChanged(nameof(WindowResizeMode));
                }
                finally
                {
                    _rulerInfo.SuppressNotifications = false;
                }
            } 
        }
        // Property for opacity, with change notification
        public double Opacity
        {
            get => _rulerInfo.Opacity;
            set
            {
                if(Math.Abs(_rulerInfo.Opacity - value) < 0.001) return;
                _rulerInfo.SuppressNotifications = true;
                try
                {
                    SetProperty(_rulerInfo.Opacity,value, v=> _rulerInfo.Opacity = v,nameof(Opacity));
                    UpdateOpacitySelection();
                }
                finally
                {
                    _rulerInfo.SuppressNotifications = false;
                }
            } 
        }
        // Property for the TopMost state
        public bool TopMost
        {
            get => _rulerInfo.TopMost;
            set
            {
                if (_rulerInfo.TopMost == value) return;
                _rulerInfo.SuppressNotifications = true;
                try
                {
                   SetProperty(_rulerInfo.TopMost,value, v=> _rulerInfo.TopMost = v,nameof(TopMost));
                }
                finally
                {
                    _rulerInfo.SuppressNotifications = false;
                }
            }
        }
        // Property for the vertical state
        public bool IsVertical
        {
            get => _rulerInfo.IsVertical;
            set
            {
                if (_rulerInfo.IsVertical == value) return;
                _rulerInfo.SuppressNotifications = true;
                try
                {
                    HandleCanvasRotation();
                    SetProperty(_rulerInfo.IsVertical, value, v => _rulerInfo.IsVertical = v, nameof(IsVertical));
                }
                finally
                {
                    _rulerInfo.SuppressNotifications = false;
                }
            }
        }
        // Property for the tooltip state
        public bool ShowToolTip
        {
            get => _rulerInfo.ShowToolTip;
            set
            {
                if (_rulerInfo.ShowToolTip == value) return;
                _rulerInfo.SuppressNotifications = true;
                try
                {
                   SetProperty(_rulerInfo.ShowToolTip,value, v=> _rulerInfo.ShowToolTip = v,nameof(ShowToolTip));
                    OnPropertyChanged(nameof(IsToolTipVisible));
                }
                finally
                {
                    _rulerInfo.SuppressNotifications = false;
                }
            }
        }
        // Property for the save type
        public SaveTypes SaveType
        {
            get => _rulerInfo.SaveType;
            set
            {
                if (_rulerInfo.SaveType == value) return;
                _rulerInfo.SuppressNotifications = true;
                try
                {
                    SetProperty(_rulerInfo.SaveType,value, v=> _rulerInfo.SaveType = v,nameof(SaveType));
                    UpdateSaveTypeSelection();
                }
                finally
                {
                    _rulerInfo.SuppressNotifications = false;
                }
            }
        }
        public bool IsMagnifierEnabled
        {
            get => _rulerInfo.Magnifier.IsEnabled;
            set
            {
                if (_rulerInfo.Magnifier.IsEnabled == value) return;
                _rulerInfo.SuppressNotifications = true;
                try
                {
                   SetProperty(_rulerInfo.Magnifier.IsEnabled,value, v=> _rulerInfo.Magnifier.IsEnabled = v,nameof(IsMagnifierEnabled));
                }
                finally
                {
                    _rulerInfo.SuppressNotifications = false;
                }
            }
        }
        public double MagnifierScale
        {
            get => _rulerInfo.Magnifier.Scale;
            set
            {
                if (Math.Abs(_rulerInfo.Magnifier.Scale - value) < 0.001) return;
                _rulerInfo.SuppressNotifications = true;
                try
                {
                  SetProperty(_rulerInfo.Magnifier.Scale,value, v=> _rulerInfo.Magnifier.Scale = v,nameof(MagnifierScale));
                    UpdateMagnifierScaleSelection();
                }
                finally
                {
                    _rulerInfo.SuppressNotifications = false;
                }

            }
        }       
        public bool IsPhysicalUnits
        {
            get
            {
                return CurrentUnit == MeasurementUnit.Inches ||
                       CurrentUnit == MeasurementUnit.Millimeters ||
                       CurrentUnit == MeasurementUnit.Centimeters;
            }
        }   
        #endregion
        #region Initialization and Update Methods
        private void InitializeUnits()
        {
            UnitsOptions = new ObservableCollection<UnitOption>
            {
                // Ensures the Unit property is set
                new UnitOption { Label = "Inches", Unit = MeasurementUnit.Inches, Value = (int)MeasurementUnit.Inches },
                new UnitOption { Label = "Millimeters", Unit = MeasurementUnit.Millimeters, Value = (int)MeasurementUnit.Millimeters },
                new UnitOption { Label = "Centimeters", Unit = MeasurementUnit.Centimeters, Value = (int)MeasurementUnit.Centimeters },
                new UnitOption { Label = "Pixels", Unit = MeasurementUnit.Pixels, Value = (int)MeasurementUnit.Pixels },
                new UnitOption { Label = "Points", Unit = MeasurementUnit.Points, Value = (int)MeasurementUnit.Points }
            };

            // Set the initially selected unit based on the model
            UpdateUnitSelection(_rulerInfo.CurrentUnit);
        }
        private void InitializeOpacities()
        {
            // Placeholder: Populate Opacities collection here for future refactor
            OpacityOptions = new ObservableCollection<OpacityOption>();
            for (int i = 5; i <= 100; i += 5)
            {
                OpacityOptions.Add(new OpacityOption
                {
                    Label = $"{i}%",
                    Value = i / 100.0,
                    IsSelected = (i / 100.0) == _rulerInfo.Opacity

                });
            }
            UpdateOpacitySelection();
        }
        private void InitializeScales()
        {
            // Placeholder: Populate Scales collection here for future refactor
            ScaleOptions = new ObservableCollection<ScaleOption>();
            double[] manualScales = new double[]
        {
                0.0,0.25,0.33,0.50,0.67,0.75,0.80,0.90,1.00,1.10,1.25,1.50,1.75,2.00,2.50,3.00,4.00,5.00
        };
            foreach (var scale in manualScales)
            {
                ScaleOptions.Add(new ScaleOption
                {
                    Label = scale == 0.0 ? "Auto" : $"{scale * 100}%",
                    Value = scale,
                    IsSelected = scale == ScaleFactor
                });
            }
            UpdateScaleSelection();
        }
        private void InitializeSaveTypes()
        {
            SaveTypesOptions = new ObservableCollection<SaveOption>
            {
                // Using the strong-typed SaveType property
                new SaveOption { Label = "Size Only", SaveType = SaveTypes.size, Value = (double)SaveTypes.size },
                new SaveOption { Label = "Location Only", SaveType = SaveTypes.location, Value = (double)SaveTypes.location },
                new SaveOption { Label = "No Settings", SaveType = SaveTypes.none, Value = (double)SaveTypes.none },
                new SaveOption { Label = "All Settings", SaveType = SaveTypes.all, Value = (double)SaveTypes.all }
            };

            // Set the default selection (e.g., AllSettings)
            SaveType = SaveTypes.none;
            UpdateSaveTypeSelection();
        }
        public void InitializeCommands()
        {
            _showSetSizeFormCommand = new RelayCommand(GetNavigateSetSizeForm);
            _resetToDefaultCommand = new RelayCommand(ResetDefault);
            _toggleLockCommand = new RelayCommand(ToggleLock);
            _exitCommand = new RelayCommand(ExitApplication);
            _toggleVerticalCommand = new RelayCommand(ToggleVertical);
            _toggleTopMostCommand = new RelayCommand(ToggleTopMost);
            _toggleToolTipCommand = new RelayCommand(ToggleToolTip);
            _setOpacityCommand = new RelayCommand(SetOpacity);
            _setSaveTypeCommand = new RelayCommand(SetSaveType);
            _showAboutCommand = new RelayCommand(NavigateAbout);
            _duplicateCommand = new RelayCommand(DuplicateRuler);            
            _setScaleCommand = new RelayCommand(SetScaleCommand);
            _setUnitCommand = new RelayCommand(SetMeasurementUnit);
            _enableMagnifierCommand = new RelayCommand(_ => ToggleMagnifier());
        }
        public void InitializeMagnifyScales()
        {
            MagnifierZoomOptions = new ObservableCollection<ScaleOption>
            {
                new ScaleOption { Label = "1.5x", Value = 1.5 },
                new ScaleOption { Label = "2x", Value = 2.0, IsSelected = true },
                new ScaleOption { Label = "3x", Value = 3.0 },
                new ScaleOption { Label = "4x", Value = 4.0 }
            };
            MagnifierScale = 2.0;
            UpdateMagnifierScaleSelection();  
        }
        internal void SetInitialState(RulerInfo initialInfo)
        {
            if (initialInfo == null)
            {
                ArgumentNullException a =  new ArgumentNullException(nameof(initialInfo), "Initial RulerInfo cannot be null.");
                _loggingService.LogError("Failed to initialize RulerViewModel with null RulerInfo.", a);
                MessageBox.Show("An error occurred while initializing the ruler. The application will now close.", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }
            _isLoadingState = true;
            _rulerInfo.SuppressNotifications = true;
            try
            { 

            // 1. Set all non-dimension/non-orientation properties directly on the model
            _rulerInfo.Opacity = initialInfo.Opacity;
            _rulerInfo.ShowToolTip = initialInfo.ShowToolTip;
            _rulerInfo.IsLocked = initialInfo.IsLocked;
            _rulerInfo.TopMost = initialInfo.TopMost;
            _rulerInfo.Top = initialInfo.Top    ;
            _rulerInfo.Left = initialInfo.Left;
            _rulerInfo.SaveType = initialInfo.SaveType;


            // 2. IMPORTANT: Set IsVertical first for reconciliation
            _rulerInfo.IsVertical = initialInfo.IsVertical;


            // 3. Reconciliation Logic for Width and Height based on IsVertical
            double w = initialInfo.Width;
            double h = initialInfo.Height;

            // Determine if the saved dimensions are 'horizontal' (width >= height)
            bool savedHorizontal = w >= h;

            // If the saved orientation clashes with the saved dimensions, swap them.
            if ((_rulerInfo.IsVertical != savedHorizontal))
            {
                // Swap dimensions in the model to match the orientation
                _rulerInfo.Width = h;
                _rulerInfo.Height = w;
            }
            else
            {
                // Dimensions are already correctly oriented
                _rulerInfo.Width = w;
                _rulerInfo.Height = h;
            }
            // 5. Trigger UI updates for all properties
            // We use OnPropertyChanged for all properties to ensure the UI updates correctly from the reconciled model state.
            OnPropertyChanged(nameof(Width));
            OnPropertyChanged(nameof(Height));
            OnPropertyChanged(nameof(IsVertical));
            OnPropertyChanged(nameof(Opacity));
            OnPropertyChanged(nameof(ShowToolTip));
            OnPropertyChanged(nameof(IsLocked));
            OnPropertyChanged(nameof(TopMost));
            OnPropertyChanged(nameof(Top));
            OnPropertyChanged(nameof(Left));
            OnPropertyChanged(nameof(SaveType));
            OnPropertyChanged(nameof(RulerMeasurementsText));
            OnPropertyChanged(nameof(ScaleFactor));
            OnPropertyChanged(nameof(IsAutoScaled));
                }
            finally
            {
                _rulerInfo.SuppressNotifications = false;
            }
            // 6. Reset flag after loading is complete
            _isLoadingState = false;
            OnPropertyChanged(string.Empty); // Notify all properties

        }
        private void UpdateUnitSelection(MeasurementUnit selectedUnit)
        {
            // Set all to false first
            UnitsOptions.ToList().ForEach(u => u.IsSelected = false);

            // Find the specific one and set it
            var target = UnitsOptions.FirstOrDefault(u => u.Unit == CurrentUnit);
            if (target != null)
            {
                target.IsSelected = true;
            }
        }
        public string MonitorDeviceId
        {
            get => _rulerInfo.MonitorDeviceId;
            set
            {
                if (_rulerInfo.MonitorDeviceId == value) return;
                _rulerInfo.SuppressNotifications = true;
                try
                {
                   SetProperty(_rulerInfo.MonitorDeviceId,value, v=> _rulerInfo.MonitorDeviceId = v,nameof(MonitorDeviceId));
                }
                finally
                {
                    _rulerInfo.SuppressNotifications = false;
                }
            }
        }
        private void UpdateSaveTypeSelection()
        {
            SaveTypesOptions.ToList().ForEach(s => s.IsSelected = false);
            var target = SaveTypesOptions.FirstOrDefault(s => s.SaveType == SaveType);
            if (target != null)
            {
                target.IsSelected = true;
            }
            }
        public void DisableNonPhysicalScales()
        {
            ScaleOptions.ToList().ForEach(s => s.IsEnabled = false);
            var target = ScaleOptions.FirstOrDefault(s => s.Value == 1.0);
            if (target != null)
            {
                target.IsEnabled = true;
                target.IsSelected = true;
                ScaleFactor = 1.0;
            }


           //UpdateScaleFlags();
        }
        public void EnableAllScales()
        {
           ScaleOptions.ToList().ForEach(s => s.IsEnabled = true);
            var target = ScaleOptions.FirstOrDefault(s => s.Value == ScaleFactor);
            if (target != null) {
                target.IsSelected = true;
            }
           // UpdateScaleFlags();
        }
        public void UpdateOpacitySelection()
        {
            OpacityOptions.ToList().ForEach(o => o.IsSelected = false);
            var target = OpacityOptions.FirstOrDefault(o => Math.Abs(o.Value - Opacity) < 0.001);
            if (target != null)
            {
                target.IsSelected = true;
            }
        }
        public void UpdateScaleSelection()
        {
            ScaleOptions.ToList().ForEach(s => s.IsSelected=false);
            var target = ScaleOptions.FirstOrDefault(s => Math.Abs(s.Value - ScaleFactor) < 0.001);
            if (target != null)
            {
                target.IsSelected = true;
            }
        }
        public void UpdateMagnifierScaleSelection()
        {
            MagnifierZoomOptions.ToList().ForEach(o => o.IsSelected = false);
            var target = MagnifierZoomOptions.FirstOrDefault(o => Math.Abs(o.Value - MagnifierScale) < 0.001);
            if (target!=null)
            {
                target.IsSelected = true;
            }
        }
        #endregion
        #region Event Handlers and Command Logic
        private void OnRulerInfoPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_rulerInfo.SuppressNotifications) return;
            // 1. Re-notify the View for the property that changed on the Model.
            OnPropertyChanged(e.PropertyName);
            switch (e.PropertyName)
            {
                case nameof(Opacity):
                    UpdateOpacitySelection();
                    OnPropertyChanged(nameof(Opacity));
                    break;
                case nameof(SaveType):
                    UpdateSaveTypeSelection();
                    OnPropertyChanged(nameof(SaveType));
                    break;
                case nameof(IsLocked):
                    OnPropertyChanged(nameof(WindowResizeMode));
                    break;
                case nameof(CurrentUnit):
                    if (IsPhysicalUnits)
                    {
                        DisableNonPhysicalScales();
                    }
                    else
                    {
                        EnableAllScales();
                    }
                    break;
                case nameof(MagnifierScale):
                    UpdateMagnifierScaleSelection();
                    break;
                default:
                    _rulerInfo.RefreshAll();
                    break;
            }

            // 3. Notify computed properties
            OnPropertyChanged(nameof(RulerMeasurementsText));              
        }
        private void ToggleMagnifier()
        {
            IsMagnifierEnabled = !IsMagnifierEnabled;
        }
        private void SetScaleCommand(object parameter)
        {
            if (parameter is ScaleOption option)
            {
                ScaleFactor = option.Value; 
            }           
        }
        private void GetNavigateSetSizeForm(object parameters)
        {
            //var s = _dialogService.ShowSetSizeDialog(this.Width, this.Height);
            //SetRulerDimensions(s.Width, s.Height);
        }
        private void DuplicateRuler(object parameters)
        {
            //RulerInfo ri = new RulerInfo();

            //RulerInfo.CopyInto(_rulerInfo, ri);
            //_dialogService.ShowNewRuler(ri);
        }
        // Logic for the ToggleLockCommand
        private void ToggleLock(object parameter)
        {
            IsLocked = !IsLocked;
        }
        public void ResetDefault(object parameter)
        {
            RulerInfo defaultRuler = RulerFactory.CreateDefault();
            _isLoadingState = true;
            RulerFactory.CopyValues(defaultRuler, _rulerInfo);
            _isLoadingState = false;
        }
        private void ExitApplication(object parameter)
        {
         //   if (parameter is Window windowToClose)
         //   {
         //   //    _persistenceService.SaveRulerState(_rulerInfo);
         ////       bool isLastRuler = _dialogService.OpenRulers.Count() == 1;
         //       windowToClose.Close();
         //       if (isLastRuler)
         //       {
         //           Application.Current.Shutdown();
         //       }
         //   }
        }
        // Logic for the ToggleVerticalCommand
        private void ToggleVertical(object parameter)
        {
            IsVertical = !IsVertical;
        }
        // Logic for the ToggleTopMostCommand
        private void ToggleTopMost(object parameter)
        {
            TopMost = !TopMost;
        }
        // Logic for the ToggleToolTipCommand
        private void ToggleToolTip(object parameter)
        {
            ShowToolTip = !ShowToolTip;
        }
        // Logic for the SetOpacityCommand
        private void SetOpacity(object parameter)
        {
           if (parameter is OpacityOption option)
            {
                Opacity= option.Value;
            }
        }
        // Logic for the SetSaveTypeCommand
        private void SetSaveType(object parameter)
        {
            if (parameter is SaveOption option)
            {
                SaveType= option.SaveType;
            }
        }
        private void SetMeasurementUnit(object parameter)
        {
            if (parameter is UnitOption unit)
            {
                UpdateUnitSelection(unit.Unit);
            }
        }
        private void NavigateAbout(object parameter)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Version version = assembly.GetName().Version;
            string message = string.Format(
                "Original Ruler implemented by Jeff Key\n" +
                "www.sliver.com\n" +
                "ruler.codeplex.com\n" +
                "Icon by Kristen Magee @ www.kbecca.com.\n" +
                "Maintained by Andrija Cacanovic\n" +
                "Hosted on \n" +
                "https://github.com/andrijac/ruler\n" +
                "Version {0}",
                $"{version.Major}.{version.Minor}.{version.Build}.{version.MajorRevision}");
            _dialogService.ShowAboutDialog(message);
        }
        #endregion
        public void SetRulerDimensions(double newWidth, double newHeight)
        {
            if (this.Width == newWidth && this.Height == newHeight)
            {
                return;
            }
            Width = newWidth;
            Height = newHeight;           
        }
        public void UpdateLocation(double left, double top)
        {
            Left = left;
            Top = top;
        }
        public ScaleMode ScalingMode
        {
            get => _rulerInfo.ScalingMode;
            set  => _rulerInfo.ScalingMode = value;
        }
        public double FinalScale
        {
            get
            {
                switch (ScalingMode)
                {
                    case ScaleMode.Physical:
                        // Use the raw hardware DPI provided by EnvironmentService
                        return _currentPhysicalScale;

                    case ScaleMode.Manual:
                        // Use the exact value the user typed/slid
                        return this.ManualScaleValue;

                    case ScaleMode.Auto:
                    default:
                        // Follow the Windows OS scaling settings
                        return _currentEffectiveScale;
                }
            }
        }
        public double CurrentPhysicalScale
        {
            get => _currentPhysicalScale;
            set
            {
                if (Math.Abs(_currentPhysicalScale - value) < 0.001) return;
                SetProperty(ref _currentPhysicalScale, value);
                if (ScalingMode == ScaleMode.Physical)
                {
                    OnPropertyChanged(nameof(FinalScale));
                }
            }
        }
        public bool IsInitialized
        {
            get => _isInitialized;
            set
            {
                SetProperty(ref _isInitialized, value);
            }
        }
        public string RulerMeasurementsText
        {
            get
            {
                return IsVertical ? $"Height: {Height} x Width: {Width}" : $"Width: {Width} x Height: {Height}";
            }
        }
        public bool IsToolTipVisible
        {
            get => ShowToolTip;
        }
        // Command properties for UI actions
        public ICommand ToggleLockCommand => _toggleLockCommand;
        public ICommand ExitCommand => _exitCommand;
        public ICommand ToggleVerticalCommand => _toggleVerticalCommand;
        public ICommand ToggleTopMostCommand => _toggleTopMostCommand;
        public ICommand ToggleToolTipCommand => _toggleToolTipCommand;
        public ICommand SetOpacityCommand => _setOpacityCommand;
        public ICommand SetSaveTypeCommand => _setSaveTypeCommand;
        public ICommand ShowSetSizeFormCommand => _showSetSizeFormCommand;
        public ICommand ShowAboutCommand => _showAboutCommand;
        public ICommand DuplicateCommand => _duplicateCommand;
        public ICommand ResetToDefaultCommand => _resetToDefaultCommand;
        public ICommand ManualScaleCommand => _manualScaleCommand;
        public ICommand ScaleCommand => _setScaleCommand;
        public ICommand SetUnitCommand => _setUnitCommand;
        public ICommand EnableMagnifierCommand => _enableMagnifierCommand;
        public void UpdateScaleFlags()
        {
            foreach (var option in ScaleOptions)
            {
                if (IsAutoScaled && option.Value == 0.0)
                {
                    ScalingMode = ScaleMode.Auto;
                    option.IsSelected = true;
                }
                else if (!IsAutoScaled && option.Value >0.0 && option.Value<1.0)
                {
                    ScalingMode = ScaleMode.Manual;
                    option.IsSelected = true;
                }
                else
                {
                    ScalingMode = ScaleMode.Physical;
                    option.IsSelected = false;
                }
            }
        }
        public Orientation RulerOrientation
        {
            get => IsVertical ? Orientation.Vertical : Orientation.Horizontal;
        }
        //public double ScaleFactor
        //{
        //    get 
        //    {
        //        if (IsAutoScaled)
        //        {
                    
        //        }
        //    }
            
        //    set
        //    {
        //        if (_rulerInfo.ScaleFactor == value )  return;
        //        _rulerInfo.SuppressNotifications = true;
        //        try
        //        {
        //           var scales = IsAutoScaled ? SystemDpiScale : value;
        //        //    _loggingService.LogInfo ($"Setting ScaleFactor to: {scales * 100}% (IsAutoScaled: {IsAutoScaled})");
        //            SetProperty(_rulerInfo.ScaleFactor, scales, v => _rulerInfo.ScaleFactor = v, nameof(ScaleFactor));
        //            UpdateScaleSelection();
        //        }
        //        catch (Exception ex)
        //        {
        //       //     _loggingService.Log
        //       //     ($"Error setting ScaleFactor: {ex.Message}",ex);
        //        }
        //        finally
        //        {
        //            _rulerInfo.SuppressNotifications = false;
        //        }
               
        //    }
        //}
        private void HandleCanvasRotation()
        {
            // Example logic: Swap width and height in the model when rotating
            double temp = _rulerInfo.Width;
            _rulerInfo.Width = _rulerInfo.Height;
            _rulerInfo.Height = temp;
        }
        public void UpdateSystemScale(double systemDpiScale)
        {
            //_loggingService.LogInfo ($"Applying Autoscale: {systemDpiScale * 100}%");

            // Only update ScaleFactor if IsAutoScaled is true, otherwise keep the manual value
            if (IsAutoScaled)
            {
                // Set the scale factor to the system DPI value
                ScaleFactor = systemDpiScale;
            }

            // Ensure UI updates if the mode has changed
           
        }
        // Logic for the ExitCommand

        public double SystemDpiScale
        {
            get => _systemDpiScale;
            set
            {
                if (_systemDpiScale == value) return;
                   SetProperty(ref _systemDpiScale, value);
                // When DPI changes, we usually need to force a redraw of the ruler
                OnPropertyChanged(nameof(ScaleFactor));
            }
        }
        //private double GetSystemDpiScale()
        //{
        //    // We pass the MainWindow to get the DPI of the current display monitor
        // //   return _environmentService.GetDpiScale(Application.Current.MainWindow);
        //}
        public void SetGuideLinePosition(double position)
        {
            GuideLinePosition = position;
        }

        internal void OnMonitorChanged(object hMonitor)
        {
            throw new NotImplementedException();
        }

        public bool IsAutoScaled
        {
            get => _rulerInfo.IsAutoScaled;
            set
            {
                if (_rulerInfo.IsAutoScaled == value) return;
                _rulerInfo.SuppressNotifications = true;
                try
                {
                    if (value==true)
                    {
                        SetProperty(_rulerInfo.IsAutoScaled, true, v => _rulerInfo.IsAutoScaled = v, nameof(IsAutoScaled));
                        UpdateScaleSelection();
                        UpdateScaleSelection();
                    }
                    else
                    {
                       SetProperty(_rulerInfo.IsAutoScaled, false, v => _rulerInfo.IsAutoScaled = v, nameof(IsAutoScaled));
                        UpdateScaleSelection();
                    }
                }
               finally
                {
                    _rulerInfo.SuppressNotifications = false;   
                }
            }
        }
        public ObservableCollection<UnitOption> UnitsOptions { get; set; }
        public ObservableCollection<SaveOption> SaveTypesOptions { get; set; }
        public ObservableCollection<OpacityOption> OpacityOptions { get; set; }
        public ObservableCollection<ScaleOption> ScaleOptions { get; set; }
        public ObservableCollection<ScaleOption> MagnifierZoomOptions { get; set; }
    }
}

using Ruler.Wpf;
using Ruler.Wpf.Common;
using Ruler.Wpf.Enums;
using Ruler.Wpf.Models;
using Ruler.Wpf.Services;
using Ruler.Wpf.Services.Persistence;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Ruler.Wpf.ViewModels
{
    // The core ViewModel class for the Ruler application
    public class RulerViewModel : ViewModelBase
    {
        // The RulerInfo class would be our Model
        private RulerInfo _rulerInfo;
        private IDialogService _dialogService;
        private ILoggingService _loggingService;
        private Point _displaylocation=new Point(0,0);

   
    

        private bool _isInitialized = false; 
       

        public ObservableCollection<UnitOption> UnitsOptions { get; set; }
         public ObservableCollection<SaveOption> SaveTypesOptions { get; set; }
        public ObservableCollection<OpacityOption> OpacityOptions { get; set; }
        public ObservableCollection<ScaleOption> ScaleOptions { get; set; }

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


        private int _horizontalMinHeight = 85;
        private int _vericalMinWidth = 93;

       

        // State variables for mouse interaction
        private Point _startPoint;
        private Size _startSize;
        private bool _isResizing;
        private bool _isMoving;
        private double _length;
        private bool _isLocked;
        private int leftMargin = 111;
        private double actualWidth = 203;
        private bool _isLoadingState;
        private double _middlewidth = 1;
        private readonly SingleRulerPersistenceService _persistenceService;
        private double _topRowHeight;
        private bool _isGuideLineVisible = false;
        private double _guideLinePosition;
        private double _minheight = 45;
        private double _minwidth = 40;
        private double _scaleFactor;
        private bool _isOnlySingleRulerVisible = false;
        private bool _isAutoScaled;
        #region Constructor
        public RulerViewModel(IDialogService dialogService, RulerInfo initialInfo, SingleRulerPersistenceService persistenceService, ILoggingService loggingService)
        {
            _dialogService = dialogService ?? throw new ArgumentException(nameof(dialogService));
            _persistenceService = persistenceService ?? throw new ArgumentException(nameof(persistenceService));
            _loggingService = loggingService ?? throw new ArgumentException(nameof(loggingService));
            _rulerInfo = initialInfo ?? throw new ArgumentException(nameof(initialInfo));            
            InitializeCommands();                    
            InitializeUnits();
            InitializeOpacities();
            InitializeScales();
            InitializeSaveTypes();
            _rulerInfo.PropertyChanged += OnRulerInfoPropertyChanged;
        }
        #endregion
        #region Properties
        public bool IsGuideLineVisible
        {
            get => _isGuideLineVisible;
            set => _isGuideLineVisible = value;
        }
        public double GuideLinePosition
        {
            get => _guideLinePosition;
            set => _guideLinePosition = value;
        }
        public ResizeMode WindowResizeMode
        {
            get => IsLocked ? ResizeMode.NoResize : ResizeMode.CanResize;
        }
        public Color GuideLineColor
        {
            get;
            set;
        } = Colors.Red;
        public SaveTypes CurrentSaveType
        {
            get => _rulerInfo.SaveType;
            set => _rulerInfo.SaveType = value;

        }
        public MeasurementUnit CurrentUnit
        {
            get => _rulerInfo.CurrentUnit;
            set => _rulerInfo.CurrentUnit = value;
        }
        // Property for the ruler's width, with change notification
        public double Width
        {
            get => _rulerInfo.Width;
            set => _rulerInfo.Width = value;
        }
        // Property for the ruler's height, with change notification
        public double Height
        {
            get => _rulerInfo.Height;
            set => _rulerInfo.Height = value;
        }
        public double Left
        {
            get => _rulerInfo.Left;
            set => _rulerInfo.Left = value;
        }
        public double Top
        {
            get => _rulerInfo.Top;
            set => _rulerInfo.Top = value;
        }
        // Property for the ruler's location, with change notification
        public Point DisplayedLocation
        {
            get => _displaylocation;
            set => _displaylocation = value;
        }
        // Property for the lock state, with change notification
        public bool IsLocked
        {
            get => _rulerInfo.IsLocked;
            set => _rulerInfo.IsLocked = value;
        }
        // Property for opacity, with change notification
        public double Opacity
        {
            get => _rulerInfo.Opacity;
            set => _rulerInfo.Opacity = value;
        }
        // Property for the TopMost state
        public bool TopMost
        {
            get => _rulerInfo.TopMost;
            set => _rulerInfo.TopMost = value;
        }
        // Property for the vertical state
        public bool IsVertical
        {
            get => _rulerInfo.IsVertical;
            set => _rulerInfo.IsVertical = value;
        }
        // Property for the tooltip state
        public bool ShowToolTip
        {
            get => _rulerInfo.ShowToolTip;
            set => _rulerInfo.ShowToolTip = value;
        }
        // Property for the save type
        public SaveTypes SaveType
        {
            get => _rulerInfo.SaveType;
            set => _rulerInfo.SaveType = value;
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
            UpdateOpacitySelection(_rulerInfo.Opacity);
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
            UpdateScaleSelection(_rulerInfo.ScaleFactor);
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
            UpdateSaveTypeSelection(CurrentSaveType);
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
        }
        internal void SetInitialState(RulerInfo initialInfo)
        {
            _isLoadingState = true;

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
            if ((_rulerInfo.IsVertical && savedHorizontal) || (!_rulerInfo.IsVertical && !savedHorizontal))
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
            OnPropertyChanged(nameof(DisplayedLocation));
            OnPropertyChanged(nameof(SaveType));
            OnPropertyChanged(nameof(RulerMeasurementsText));
            OnPropertyChanged(nameof(ScaleFactor));
            OnPropertyChanged(nameof(IsAutoScaled));
            // 6. Reset flag after loading is complete
            _isLoadingState = false;

        }
        private void UpdateUnitSelection(MeasurementUnit selectedUnit)
        {
            foreach (var unit in UnitsOptions)
            {
                // Ensures type-safe comparison using the Unit property
               if (unit.Unit == selectedUnit)
                {
                    unit.IsSelected = true;
                    CurrentUnit = selectedUnit;
                }
                else
                {
                    unit.IsSelected = false;
                }
            }
        }
        private void UpdateSaveTypeSelection(SaveTypes selectedType)
        {
            foreach (var option in SaveTypesOptions)
            {
                // Ensures only the selected item is marked as checked (mutually exclusive)
                // Now uses the strongly-typed SaveType property for comparison
                if (option.SaveType == selectedType)
                {
                    option.IsSelected = true;
                    SaveType = selectedType;
                   
                }
                else
                {
                    option.IsSelected = false;
                }
            }
        }
        public void UpdateOpacitySelection(double selectedOpacity)
        {
            foreach (var option in OpacityOptions)
            {
                if (option.Value==selectedOpacity)
                {
                    option.IsSelected = true;
                    Opacity = selectedOpacity;
                }
                else
                {
                    option.IsSelected = false;
                }
            }
        }
        public void UpdateScaleSelection(double selectedScale)
        {
            foreach (var option in ScaleOptions)
            {
                if (option.Value==selectedScale)
                {
                    if (selectedScale == 0.0)
                    {
                        IsAutoScaled = true;
                    }
                    else
                    {
                        IsAutoScaled = false;
                    }
                    option.IsSelected = true;
                    ScaleFactor = selectedScale;                   
                }
                else
                {
                    option.IsSelected = false; 
                }
            }
        }
        #endregion
        #region Event Handlers and Command Logic
        private void OnRulerInfoPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // 1. Re-notify the View for the property that changed on the Model.
            OnPropertyChanged(e.PropertyName);

            // 2. Critical: If the Opacity changed, update all the menu item flags
            if (e.PropertyName == nameof(Opacity))
            {
                UpdateOpacitySelection(_rulerInfo.Opacity);
            }
            if (e.PropertyName == nameof(CurrentUnit))
            {
                UpdateUnitSelection(_rulerInfo.CurrentUnit);
            }
            if (e.PropertyName == nameof(SaveType))
            {
                UpdateSaveTypeSelection(_rulerInfo.SaveType);
            }
            if (e.PropertyName == nameof(ScaleFactor))
            {
                UpdateScaleSelection(_rulerInfo.ScaleFactor);
            }
            if (e.PropertyName == nameof(IsVertical))
            {
                OnPropertyChanged(nameof(RulerOrientation));
            }

            // 3. Notify computed properties
            OnPropertyChanged(nameof(RulerMeasurementsText));              
        }        
       
        private void SetScaleCommand(object parameter)
        {
            if (parameter is ScaleOption option)
            {
                UpdateScaleSelection(option.Value);
            }           
        }
       
        private void GetNavigateSetSizeForm(object parameters)
        {
            var s = _dialogService.ShowSetSizeDialog(this.Width, this.Height);
            SetRulerDimensions(s.Width, s.Height);
        }
        private void DuplicateRuler(object parameters)
        {
            RulerInfo ri = new RulerInfo();

            RulerInfo.CopyInto(_rulerInfo, ri);
            _dialogService.ShowNewRuler(ri);
        }
        // Logic for the ToggleLockCommand
        private void ToggleLock(object parameter)
        {
            IsLocked = !IsLocked;
        }
        public void ResetDefault(object parameter)
        {
            RulerInfo defaultRuler = RulerInfo.GetDefaultRulerInfo();
            _isLoadingState = true;
            RulerInfo.CopyInto(defaultRuler, _rulerInfo);
            _isLoadingState = false;
        }
        private void ExitApplication(object parameter)
        {
            if (parameter is Window windowToClose)
            {
                _persistenceService.SaveRulerState(_rulerInfo);
                bool isLastRuler = _dialogService.OpenRulers.Count() == 1;
                windowToClose.Close();
                if (isLastRuler)
                {
                    Application.Current.Shutdown();
                }
            }
        }
        // Logic for the ToggleVerticalCommand
        private void ToggleVertical(object parameter)
        {
            IsVertical = !IsVertical;
            double oldWidth = Width;
            double oldHeight = Height;
            SetRulerDimensions(oldHeight, oldWidth);
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
                UpdateOpacitySelection(option.Value);
            }
        }
        // Logic for the SetSaveTypeCommand
        private void SetSaveType(object parameter)
        {
            if (parameter is SaveOption option)
            {
                UpdateSaveTypeSelection(option.SaveType);
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
            MessageBox.Show(message, "About Ruler", MessageBoxButton.OK, MessageBoxImage.Information);
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


        public void UpdateScaleFlags()
        {
           foreach (var option in ScaleOptions)
            {
                option.IsSelected = option.Value == ScaleFactor;
            }
        }
        public void SetScaleFactorCommand(object parameter)
        {
            if (parameter != null && parameter is ScaleOption scales)
            {


                foreach (var option in ScaleOptions)
                {
                    if (option.Value == 0.0)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            IsAutoScaled = true;
                        });
                        ScaleFactor = option.Value;
                        break;
                    }
                    else if (option.Value == scales.Value)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            IsAutoScaled = false;
                        });
                        ScaleFactor = option.Value;
                        break;
                    }
                }
            }
           
            UpdateScaleFlags();
        }
       public Orientation RulerOrientation
        {
            get => IsVertical ? Orientation.Vertical : Orientation.Horizontal;
        }
        public double ScaleFactor
        {
            get => _rulerInfo.ScaleFactor;
            set
            {
                if (_rulerInfo.ScaleFactor!= value)
                {                    
                    _rulerInfo.ScaleFactor = value;                  
                    
                }
            }
        }
        public void SetAutoScaleFactor(double systemDpiScale)
        {
            _loggingService.LogInfo ($"Applying Autoscale: {systemDpiScale * 100}%");

            // Only update ScaleFactor if IsAutoScaled is true, otherwise keep the manual value
            if (IsAutoScaled)
            {
                // Set the scale factor to the system DPI value
                ScaleFactor = systemDpiScale;
            }

            // Ensure UI updates if the mode has changed
           
        }   
        // Logic for the ExitCommand

       

        public void SetGuideLinePosition(double position)
        {
            GuideLinePosition = position;
        }

        public bool IsAutoScaled
        {
            get => _rulerInfo.IsAutoScaled;
            set
            {
                if (_rulerInfo.IsAutoScaled != value)
                {
                    _isAutoScaled = value;
                    _rulerInfo.IsAutoScaled = value;
                    OnPropertyChanged();
                    // Ensure manual scale checkmarks are updated when auto scale changes
                    UpdateScaleFlags();
                }
            }
        }

      


    }
}

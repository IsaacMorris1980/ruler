using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Ruler.Shared.Commands;
using Ruler.Shared.Interfaces;
using Ruler.Shared.Models;

namespace Ruler.Wpf.ViewModels
{
    public class MainViewModel : ModelBase,IMainViewModel
    {
        private RulerInfo _rulerInfo;
        private bool _showToolTips = true;
        private MenuItemModel _updateMenuItem;
        private bool _isUpdateAvailable = false;
        private string _owner = "IsaacMorris1980";
        private string _repo = "ruler";

        public RulerInfo RulerData => _rulerInfo;
        public ObservableCollection<MenuItemModel> MenuItems { get; set; } = new ObservableCollection<MenuItemModel>();

        // Events to request UI actions from the View (Window)
        public event EventHandler CloseRequested;
        public event EventHandler<string> ShowMessageRequested;
        public event EventHandler ShowShortcutWindowRequested;
        public event EventHandler ShowUpdateWindowRequested;
        public event EventHandler<int[]> ShowSetSizeWindowRequested;
        public event EventHandler ToggleTopMostRequested;
        public event EventHandler InvalidateViewRequested;
        public event EventHandler<double> OpacityChangedRequested;
        public event EventHandler ToggleMagnifierRequested;
        public event EventHandler<RulerInfo> DuplicateRequested;

        public bool ShowToolTips
        {
            get => _showToolTips;
            set => SetProperty(ref _showToolTips, value);
        }

        // Commands
        public ICommand ToggleTopMostCommand { get; private set; }
        public ICommand ToggleLockResizingCommand { get; private set; }
        public ICommand ToggleOrientationCommand { get; private set; }
        public ICommand ToggleToolTipCommand { get; private set; }
        public ICommand ToggleSetSizeCommand { get; private set; }
        public ICommand ToggleDuplicateCommand { get; private set; }
        public ICommand ToggleShowGuidelineCommand { get; private set; }
        public ICommand ToggleResetToDefaultCommand { get; private set; }
        public ICommand ToggleAboutCommand { get; private set; }
        public ICommand ToggleCloseCommand { get; private set; }
        public ICommand ToggleExitCommand { get; private set; }
        public ICommand ToggleShowShortcutsCommand { get; private set; }
        public ICommand SetOpacityCommand { get; private set; }
        public ICommand MaxOpacityCommand { get; private set; }
        public ICommand MinOpacityCommand { get; private set; }
        public ICommand ToggleMagnifierCommand { get; private set; }
        public ICommand SetSaveTypeCommand { get; private set; }
        public ICommand SetMagnficationScaleCommand { get; private set; }
        public ICommand CheckOrApplyUpdateCommand { get; private set; }

        private readonly IRulerFactory _rulerFactory;

        private readonly IRulerRegistry _rulerRegistry;

        public MainViewModel(RulerInfo info, IRulerFactory rulerFactory,IRulerRegistry rulerRegistry)
        {
            _rulerInfo = info;
            _rulerFactory = rulerFactory;
            _rulerRegistry = rulerRegistry;
            PopulateCommands();
            PopulateMenu();
            _rulerRegistry.Register(this);
        }

        public void PopulateCommands()
        {
            ToggleTopMostCommand = new DelegateCommand(_ => {
                _rulerInfo.TopMost = !_rulerInfo.TopMost;
                ToggleTopMostRequested?.Invoke(this, EventArgs.Empty);
                var menuItem = MenuItems.FirstOrDefault(i => i.Header == "Stay On Top");
                if (menuItem != null) menuItem.IsChecked = _rulerInfo.TopMost;
                InvalidateViewRequested?.Invoke(this, EventArgs.Empty);
            });

            ToggleLockResizingCommand = new DelegateCommand(_ =>
            {
                _rulerInfo.IsLocked = !_rulerInfo.IsLocked;
                var menuItem = MenuItems.FirstOrDefault(i => i.Header == "Lock Resizing");
                if (menuItem != null) menuItem.IsChecked = _rulerInfo.IsLocked;
                InvalidateViewRequested?.Invoke(this, EventArgs.Empty);
            });

            ToggleOrientationCommand = new DelegateCommand(_ =>
            {
                _rulerInfo.ToggleOrientation();
                var menuItem = MenuItems.FirstOrDefault(i => i.Header == "Is Vertical?");
                if (menuItem != null) menuItem.IsChecked = _rulerInfo.IsVertical;
                InvalidateViewRequested?.Invoke(this, EventArgs.Empty);
            });

            ToggleToolTipCommand = new DelegateCommand(_ =>
            {
                _rulerInfo.ShowToolTip = !_rulerInfo.ShowToolTip;
                var menuItem = MenuItems.FirstOrDefault(i => i.Header == "Show ToolTip");
                if (menuItem != null) menuItem.IsChecked = _rulerInfo.ShowToolTip;
                InvalidateViewRequested?.Invoke(this, EventArgs.Empty);
            });

            ToggleSetSizeCommand = new DelegateCommand(_ =>
            {
                ShowSetSizeWindowRequested?.Invoke(this, new int[] { _rulerInfo.Width, _rulerInfo.Height });
            });

            ToggleDuplicateCommand = new DelegateCommand(_ =>
            {
                DuplicateRequested?.Invoke(this, _rulerInfo);
            });

            ToggleShowGuidelineCommand = new DelegateCommand(_ =>
            {
                _rulerInfo.Guideline.IsEnabled = !_rulerInfo.Guideline.IsEnabled;
                var menuItem = MenuItems.FirstOrDefault(i => i.Header == "Show Guideline");
                if (menuItem != null) menuItem.IsChecked = _rulerInfo.Guideline.IsEnabled;
                InvalidateViewRequested?.Invoke(this, EventArgs.Empty);
            });

            ToggleResetToDefaultCommand = new DelegateCommand(_ =>
            {
                RulerInfo info = _rulerFactory.CreateDefault();
                _rulerFactory.CopyValues(info, _rulerInfo);
                InvalidateViewRequested?.Invoke(this, EventArgs.Empty);
            });

            ToggleAboutCommand = new DelegateCommand(_ =>
            {
                string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
                string message = $"Original Ruler implemented by Jeff Key\nVersion {version}";
                ShowMessageRequested?.Invoke(this, message);
            });

            ToggleCloseCommand = new DelegateCommand(_ => CloseRequested?.Invoke(this, EventArgs.Empty));
            ToggleExitCommand = new DelegateCommand(_ => System.Windows.Application.Current.Shutdown());

            ToggleShowShortcutsCommand = new DelegateCommand(_ => ShowShortcutWindowRequested?.Invoke(this, EventArgs.Empty));

            SetMagnficationScaleCommand = new DelegateCommand<object>(param =>
            {
                if (param is MenuItemModel model && model.Value is double dVal && dVal > 0)
                {
                    _rulerInfo.Magnifier.ZoomLevel = dVal;
                }
            });

            SetOpacityCommand = new DelegateCommand<object>(parameter =>
            {
                double targetOpacity = _rulerInfo.Opacity;
                if (parameter is MenuItemModel model && model.Value is double modelVal)
                {
                    targetOpacity = modelVal;
                }
                double finalOpacity = Math.Max(0.1, Math.Min(1.0, targetOpacity));
                _rulerInfo.Opacity = finalOpacity;
                OpacityChangedRequested?.Invoke(this, finalOpacity);
                InvalidateViewRequested?.Invoke(this, EventArgs.Empty);
            });

            MaxOpacityCommand = new DelegateCommand(_ =>
            {
                _rulerInfo.Opacity = 1.0;
                OpacityChangedRequested?.Invoke(this, 1.0);
                InvalidateViewRequested?.Invoke(this, EventArgs.Empty);
            });

            MinOpacityCommand = new DelegateCommand(_ =>
            {
                _rulerInfo.Opacity = 0.1;
                OpacityChangedRequested?.Invoke(this, 0.1);
                InvalidateViewRequested?.Invoke(this, EventArgs.Empty);
            });

            ToggleMagnifierCommand = new DelegateCommand(_ => ToggleMagnifierRequested?.Invoke(this, EventArgs.Empty));

            CheckOrApplyUpdateCommand = new DelegateCommand(_ => ShowUpdateWindowRequested?.Invoke(this, EventArgs.Empty));
        }

        public void PopulateMenu()
        {
            MenuItems.Clear();
            MenuItems.Add(new MenuItemModel { Header = "Stay On Top", IsCheckable = true, IsChecked = _rulerInfo.TopMost, Command = ToggleTopMostCommand });
            MenuItems.Add(new MenuItemModel { Header = "Lock Resizing", IsCheckable = true, IsChecked = _rulerInfo.IsLocked, Command = ToggleLockResizingCommand });
            MenuItems.Add(new MenuItemModel { Header = "Is Vertical?", IsCheckable = true, IsChecked = _rulerInfo.IsVertical, Command = ToggleOrientationCommand });
            MenuItems.Add(new MenuItemModel { Header = "Set Ruler Size", Command = ToggleSetSizeCommand });
            MenuItems.Add(new MenuItemModel { Header = "Show ToolTip", IsCheckable = true, IsChecked = _rulerInfo.ShowToolTip, Command = ToggleToolTipCommand });
            MenuItems.Add(new MenuItemModel { Header = "Duplicate Ruler", Command = ToggleDuplicateCommand });
            MenuItems.Add(new MenuItemModel { Header = "Show Guideline", IsCheckable = true, IsChecked = _rulerInfo.Guideline.IsEnabled, Command = ToggleShowGuidelineCommand });

            _updateMenuItem = new MenuItemModel { Header = "Check for Updates", Command = CheckOrApplyUpdateCommand };
            MenuItems.Add(_updateMenuItem);

            MenuItems.Add(new MenuItemModel { Header = "Show Magnifier", IsCheckable = true, IsChecked = _rulerInfo.Magnifier.IsActive, Command = ToggleMagnifierCommand });
            MenuItems.Add(new MenuItemModel { Header = "Reset to Default", Command = ToggleResetToDefaultCommand });
            MenuItems.Add(new MenuItemModel { Header = "Show About", Command = ToggleAboutCommand });
            MenuItems.Add(new MenuItemModel { Header = "Close Ruler", Command = ToggleCloseCommand });
            MenuItems.Add(new MenuItemModel { Header = "Exit Application", Command = ToggleExitCommand });
        }

        public void RequestClose()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

    }
}

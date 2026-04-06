using System;

namespace Ruler.Shared
{
    public class RulerInfo:ModelBase, IRulerInfo
    {
        #region variables
        private double _width = 500;
        private double _height = 150;
        private double _left = 100;
        private double _top = 100;
        private bool _isVertical = false;
        private double _opacity = 1.0;
        private bool _showToolTip = true;
        private bool _isLocked = false;
        private bool _topMost = true;
        private SaveTypes _saveType = SaveTypes.none;
        private MeasurementUnit _currentUnit = MeasurementUnit.Pixels;
        private RulerGuideline _guideline = new RulerGuideline();
        private RulerMagnifier _magnifier = new RulerMagnifier();
        private Guid _id;
        private double _manualScale = 1.0;
        private string _associatedMonitorProfile;
        private ScaleMode _scaleMode = ScaleMode.Auto;
        #endregion

        #region properties
        public double Width
        {
            get=> _width;
            set=>SetProperty(ref _width,value);
        }
        public double Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }               
        public bool IsVertical
        {
            get => _isVertical;
            set => SetProperty(ref _isVertical, value);
        }
        public double Opacity
        {
           get => _opacity;
            set => SetProperty(ref _opacity, value);
        }
        public bool ShowToolTip
        {
            get => _showToolTip;
            set => SetProperty(ref _showToolTip, value);
        }
        public bool IsLocked
        {
            get => _isLocked;
            set => SetProperty(ref _isLocked, value);
        }
        public bool TopMost
        {
            get=> _topMost;
            set => SetProperty(ref _topMost, value);
        }
        public double Left
        {
            get=> _left;
            set => SetProperty(ref _left, value);
        }
        public double Top
        {
            get=> _top;
            set => SetProperty(ref _top, value);
        }      
        public SaveTypes SaveType
        {
            get=>  _saveType;
            set=> SetProperty(ref _saveType, value);
        }       
        public double ManualScale
        {
            get=> _manualScale;
            set=>SetProperty(ref _manualScale,value);
        }
        public MeasurementUnit CurrentUnit  
        {
           get => _currentUnit;
           set => SetProperty(ref _currentUnit, value);
        }
        public RulerGuideline Guideline
        {
            get => _guideline;
            set => SetProperty(ref _guideline, value);
        }
        public RulerMagnifier Magnifier
        {
            get => _magnifier;
            set => SetProperty(ref _magnifier, value);
        }
        public string MonitorDeviceId
        {
            get => _associatedMonitorProfile;
            set => SetProperty(ref _associatedMonitorProfile, value);
        }
      public Guid Id
        {
            get
            {
                if (_id == Guid.Empty)
                {
                    _id = Guid.NewGuid();
                }
                return _id;
            }
            set => SetProperty(ref _id, value);
        }
        public ScaleMode ScalingMode
        {
            get => _scaleMode;
            set => SetProperty(ref _scaleMode, value);
        }
        #endregion
    }
}

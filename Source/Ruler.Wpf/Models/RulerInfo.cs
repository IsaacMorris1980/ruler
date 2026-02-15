using Ruler.Wpf.Common;
using Ruler.Wpf.Enums;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

namespace Ruler.Wpf.Models
{
    public class RulerInfo:ModelBase, IRulerInfo
    {
        private double _width = 500;
        private double _height = 150;
        private double _left = 100;
        private double _top = 100;
        private bool _isVertical = false;
        private double _opacity = 1.0;
        private bool _showToolTip = true;
        private bool _isLocked = false;
        private bool _topMost = true;
        private string _saveType = "none";
        private double _scaleFactor = 1.0;
        private bool _isAutoScaled = true; 
        private bool _isZoomEnabled = false;
        private MeasurementUnit _currentUnit = MeasurementUnit.Pixels;
        private double _zoomFactor = 1.0;
        private bool _isMagnifierEnabled = false;
        private double _magnificationScale = 2.0;
        private double _guideLinePosition = 0.0;
        private bool _isGuideLineVisible = false;
        private bool _isGuidelineLocked = false;
        private Color _guidelineColor = Colors.Red;
        private string _monitorDeviceId = Screen.PrimaryScreen.DeviceName;

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
            get=> (SaveTypes)Enum.Parse(typeof(SaveTypes), _saveType);
            set=> SetProperty(ref _saveType, value.ToString());
        }       
        public double ScaleFactor
        {
            get=> _scaleFactor;
            set=>SetProperty(ref _scaleFactor,value);
        }
        public bool IsAutoScaled
        {
            get => _isAutoScaled;
            set
            {
                if (SetProperty(ref _isAutoScaled, value))
                {
                    // If your ViewModel is also listening to this, ensure it doesn't
                    // call this setter again with the same value.
                };
            }
        }
        public MeasurementUnit CurrentUnit  
        {
           get => _currentUnit;
           set => SetProperty(ref _currentUnit, value);
        }
        public double ZoomFactor
        {
            get =>  _zoomFactor;            
            set => SetProperty(ref _zoomFactor, value);
        }
        public bool IsZoomEnabled
        {
            get => _isZoomEnabled;
            set => SetProperty(ref _isZoomEnabled, value);
        }
        public bool IsMagnifierEnabled
        {
            get => _isMagnifierEnabled;
            set => SetProperty(ref _isMagnifierEnabled, value);
        }
        public double MagnificationScale
        {
            get => _magnificationScale;
            set => SetProperty(ref _magnificationScale, value);
        }
        public double GuideLinePosition
        {
            get => _guideLinePosition;
            set => SetProperty(ref _guideLinePosition, value);
        }
        public bool IsGuideLineVisible
        {
            get => _isGuideLineVisible;
            set => SetProperty(ref _isGuideLineVisible, value);
        }
        public Color GuidelineColor
        {
            get => _guidelineColor;
            set => SetProperty(ref _guidelineColor, value);
        }
        public bool IsGuidelineLocked
        {
            get => _isGuidelineLocked;
            set => SetProperty(ref _isGuidelineLocked, value);
        }
        public string MonitorDeviceId
        {
            get => _monitorDeviceId;
            set => SetProperty(ref _monitorDeviceId, value);
        }
      

        public static RulerInfo GetDefaultRulerInfo()
        {

            RulerInfo rulerInfo = new RulerInfo();

            // Suppress while initializing to prevent partial-state updates
            rulerInfo.SuppressNotifications = true;
            try
            {
                rulerInfo.Width = 400;
                rulerInfo.Height = 75;
                rulerInfo.Opacity = 0.60;
                rulerInfo.ShowToolTip = true;
                rulerInfo.IsLocked = false;
                rulerInfo.IsVertical = false;
                rulerInfo.TopMost = true;
                rulerInfo.Left = 0;
                rulerInfo.Top = 0;
                rulerInfo.SaveType = SaveTypes.none;
                rulerInfo.ScaleFactor = 1.0;
                rulerInfo.IsAutoScaled = false;
                rulerInfo.CurrentUnit = MeasurementUnit.Pixels;
                rulerInfo.ZoomFactor = 1.0;
                rulerInfo.IsZoomEnabled = false;
                rulerInfo.IsMagnifierEnabled = false;
                rulerInfo.MagnificationScale = 2.0;
                rulerInfo.GuideLinePosition = 0.0;
                rulerInfo.IsGuideLineVisible = false;
                rulerInfo.IsGuidelineLocked = false;
                rulerInfo.GuidelineColor = Colors.Red;
                rulerInfo.MonitorDeviceId = Screen.PrimaryScreen.DeviceName;
            }
            finally
            {
                rulerInfo.SuppressNotifications = false;
            }

            return rulerInfo;
        }

        public static void CopyInto(IRulerInfo source, IRulerInfo targetInstance)
        {
            if (source == null || targetInstance == null) return;

            // Handle SuppressNotifications if the objects are RulerInfo instances
            var sourceModel = source as RulerInfo;
            var targetModel = targetInstance as RulerInfo;

            if (sourceModel != null) sourceModel.SuppressNotifications = true;
            if (targetModel != null) targetModel.SuppressNotifications = true;

            try
            {
                // Get all properties defined specifically in the IRulerInfo interface
                PropertyInfo[] properties = typeof(IRulerInfo).GetProperties(
                    BindingFlags.Public | BindingFlags.Instance);

                foreach (PropertyInfo prop in properties)
                {
                    // Check if the property can be read from source and written to target
                    if (prop.CanRead && prop.CanWrite)
                    {
                        object value = prop.GetValue(source);
                        prop.SetValue(targetInstance, value);
                    }
                }
            }
            finally
            {
                if (sourceModel != null) sourceModel.SuppressNotifications = false;
                if (targetModel != null) targetModel.SuppressNotifications = false;
            }
        
        }
    }
}

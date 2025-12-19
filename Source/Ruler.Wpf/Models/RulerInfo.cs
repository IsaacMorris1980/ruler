using Ruler.Wpf.Common;
using Ruler.Wpf.Enums;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
            get=>_isAutoScaled;
            set => SetProperty(ref _isAutoScaled,value);
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
        public static RulerInfo GetDefaultRulerInfo()
        {
            RulerInfo rulerInfo = new RulerInfo
            {
                Width = 400,
                Height = 75,
                Opacity = 0.60,
                ShowToolTip = true,
                IsLocked = false,
                IsVertical = false,
                TopMost = true,
                Left = 0,
                Top = 0,               
                SaveType = SaveTypes.none,
                ScaleFactor = 1.0,
                IsAutoScaled = false,
                CurrentUnit = MeasurementUnit.Pixels,
              ZoomFactor = 1.0,
                IsZoomEnabled = false,
               
            };

            return rulerInfo;
        }

        public static void CopyInto(IRulerInfo source, IRulerInfo targetInstance)
        {
            targetInstance.Width = source.Width;
            targetInstance.Height = source.Height;
            targetInstance.IsVertical = source.IsVertical;
            targetInstance.Opacity = source.Opacity;
            targetInstance.ShowToolTip = source.ShowToolTip;
            targetInstance.IsLocked = source.IsLocked;
            targetInstance.TopMost = source.TopMost;
            targetInstance.Left = source.Left;
            targetInstance.Top = source.Top;
            targetInstance.SaveType = source.SaveType;
            targetInstance.ScaleFactor = source.ScaleFactor;
            targetInstance.IsAutoScaled = source.IsAutoScaled;
            targetInstance.CurrentUnit = source.CurrentUnit;
            targetInstance.ZoomFactor = source.ZoomFactor;           
            targetInstance.IsZoomEnabled = source.IsZoomEnabled;
        }     

    }
}

using Ruler.Wpf.Common;
using Ruler.Wpf.Enums;

using System.Security.AccessControl;
using System.Windows;
using System.Windows.Media;


namespace Ruler.Wpf.Models
{
    public interface IRulerInfo
    {
        double Width
        {
            get;
            set;
        }

        double Height
        {
            get;
            set;
        }
        public double Left
        {
            get;
            set;
        }
        public double Top
        {
            get;
            set;
        }

        bool IsVertical
        {
            get;
            set;
        }

        double Opacity
        {
            get;
            set;
        }

        bool ShowToolTip
        {
            get;
            set;
        }

        bool IsLocked
        {
            get;
            set;
        }

        bool TopMost
        {
            get;
            set;
        }
        SaveTypes SaveType
        {
            get;
            set;
        }
        double  ScaleFactor
        {
            get;
            set;
        }
         bool IsAutoScaled
        {
            get;
            set;
        }
        bool IsZoomEnabled
        {
            get;
            set;
        }
        MeasurementUnit CurrentUnit
        {
            get;
            set;
        }
        // bool IsHighContrast { get; set; }
        double ZoomFactor 
        { 
            get;
            set; 
        }
        bool IsMagnifierEnabled
        {
            get;
            set;
        }
        double MagnificationScale
        {
            get;
            set;
        }
        double GuideLinePosition
        {
            get;
            set;
        }
        bool IsGuideLineVisible
        {
            get;
            set;
        }
        Color GuidelineColor
        {
            get;
            set;
        }
        bool IsGuidelineLocked
        {
            get;
            set;
        }
        string MonitorDeviceId
        {
            get;
            set;
        }
    }
}
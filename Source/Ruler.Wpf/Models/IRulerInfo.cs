using Ruler.Wpf.Common;
using Ruler.Wpf.Enums;

using System.Security.AccessControl;
using System.Windows;


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
       double Left
        {
            get;
            set;
        }
        double Top
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

       // bool IsHighContrast { get; set; }
        double ZoomFactor 
        { 
            get;
            set; 
        }
        MeasurementUnit CurrentUnit 
        {
            get;
            set;
        }
       bool IsZoomEnabled
        {
            get;
            set;
        }

    }
}
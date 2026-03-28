using Ruler.Wpf.Common;
using Ruler.Wpf.Enums;

using System;
using System.Security.AccessControl;
using System.Windows;
using System.Windows.Media;
using Ruler.Shared.Enums;

namespace Ruler.Shared.Interfaces
{
    public interface IRulerInfo
    {
        #region interface properties
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
        double  ManualScale
        {
            get;
            set;
        }
        MeasurementUnit CurrentUnit
        {
            get;
            set;
        }
       
        Guid Id
        {
            get;
            set;
        }
        string MonitorDeviceId
        {
            get;
            set;
        }
        ScaleMode ScalingMode
        {
            get;
            set;
        }
        #endregion
    }
}
using System;
using Ruler.Contracts.Enums;


namespace Ruler.Contracts.Models
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
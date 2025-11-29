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
    public class RulerInfo:IRulerInfo, INotifyPropertyChanged
    {
        public double Width
        {
            get;
            set;
        }

        public double Height
        {
            get;
            set;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public bool IsVertical
        {
            get;
            set;
        }

        public double Opacity
        {
            get;
            set;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public bool ShowToolTip
        {
            get;
            set;
        }

        /// <summary>
        /// TODO
        /// </summary>
        public bool IsLocked
        {
            get;
            set;
        }

        public bool TopMost
        {
            get;
            set;
        }
        public double LocationX
        {
            get;
            set;
        }
        public double LocationY
        {
            get;
            set;
        }
        public Point DisplayedLocation
        {
            get
            {
                return new Point(LocationX, LocationY);
            }
            set
            {
                LocationX = value.X;
                LocationY = value.Y;
            }
        }   
        public SaveTypes SaveType
        {
            get;
            set;
        }      
        public bool IsGuideLineVisible 
        { 
            get;
            set; 
        }
        public double GuideLinePosition 
        { 
            get;
            set; 
        }
        public double ScaleFactor
        {
            get;
            set;
        }=1.0;
        public bool IsAutoScaled 
        { 
            get;
            set;
        }=false;
        private RulerScale _selectedScale;

        public RulerScale SelectedScale
        {
            get => _selectedScale;
            set
            {
                if (_selectedScale != value)
                {
                    _selectedScale = value;
                    OnPropertyChanged();
                }
            }
        }
        private MeasurementUnit _currentUnit;
        private double _zoomFactor = 1.0;
        public MeasurementUnit CurrentUnit
        {
            get
            {
                return _currentUnit;
            }
            set
            {
                if (_currentUnit != value)
                {
                    _currentUnit = value;
                    OnPropertyChanged();
                }
            }
        }
        public double ZoomFactor
        {
            get
            {
                return _zoomFactor;
            }
            set
            {
                if (_zoomFactor != value)
                {
                    _zoomFactor = value;
                    OnPropertyChanged();
                }
            }
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
                LocationX = 0,
                LocationY = 0,
                SaveType = SaveTypes.none,
                ScaleFactor = 1.0,
                IsAutoScaled = false
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
            targetInstance.LocationX = source.LocationX;
            targetInstance.LocationY = source.LocationY;
            targetInstance.SaveType = source.SaveType;
            targetInstance.ScaleFactor = source.ScaleFactor;
            targetInstance.IsAutoScaled = source.IsAutoScaled;
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}

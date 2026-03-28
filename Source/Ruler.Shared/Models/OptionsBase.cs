using Ruler.Shared.Enums;
using Ruler.Shared.Models;
using Ruler.Wpf.Enums;
using Ruler.Shared.Common.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Models
{
    public abstract class OptionBase : ModelBase
    {
        private bool _isSelected=false;
        private bool _isEnabled=true;


        public double Value { get; set; }

        
        public string Label { get; set; }

        
        public bool IsSelected
        {
            get => _isSelected;
            set
            {

                SetProperty(ref _isSelected, value);
            }
        }
        public bool IsEnabled { get=> _isEnabled; set=> SetProperty(ref _isEnabled,value); }


    }
    public class ScaleOption : OptionBase
    {
        public bool IsAuto { get; set; }

        public ScaleOption(double scale)
        {
            Value = scale;
            Label =scale==0?"Auto":$"{scale * 100}%";
            IsAuto = scale==0?true:false;
        }
        // Helper to create a standard option
        //public static ScaleOption CreateManual(double scale) => new ScaleOption
        //{
        //    Value = scale,
        //    Label = $"{scale * 100}%",
        //    IsAuto = false
        //};

        //// Helper to create the Auto option
        //public static ScaleOption CreateAuto() => new ScaleOption
        //{
        //    Value = 0,
        //    Label = "Auto",
        //    IsAuto = true
        //};
    }

    public class OpacityOption : OptionBase
    {
        public OpacityOption(double opacity)
        {
            Value = opacity;
            Label = $"{(int)(opacity * 100)}%";
        }
      //  public string PercentageLabel => $"{(double)Value * 100}%";
    }

    public class UnitOption : OptionBase
    {
        public MeasurementUnit Unit { get; set; }

        public UnitOption(MeasurementUnit unit)
        {
            Unit = unit;
            Label = unit.GetDescription(); // Auto-generate "Pixels", "Centimeters"
        }


    }

    public class SaveOption : OptionBase
    {
        public SaveTypes SaveType { get; set; }

        public SaveOption(SaveTypes type)
        {
                    SaveType = type;
            Label = type.GetDescription(); // Auto-generate "All", "Location", etc.
        }
    }
}

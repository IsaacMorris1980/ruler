using Ruler.Wpf.Common;
using Ruler.Wpf.Enums;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Wpf.Models
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

        // Helper to create a standard option
        public static ScaleOption CreateManual(double scale) => new ScaleOption
        {
            Value = scale,
            Label = $"{scale * 100}%",
            IsAuto = false
        };

        // Helper to create the Auto option
        public static ScaleOption CreateAuto() => new ScaleOption
        {
            Value = 0,
            Label = "Auto",
            IsAuto = true
        };
    }

    public class OpacityOption : OptionBase
    {
        public string PercentageLabel => $"{(double)Value * 100}%";
    }

    public class UnitOption : OptionBase
    {
        public MeasurementUnit Unit { get; set; }

        public static UnitOption Create(MeasurementUnit unit, MeasurementUnit current) => new UnitOption
        {
            Unit = unit,
            Label = unit.GetDescription(), // Auto-generate "Pixels", "Centimeters"
            IsSelected = unit == current
        };
    }

    public class SaveOption : OptionBase
    {
        public SaveTypes SaveType { get; set; }

        public static SaveOption Create(SaveTypes type, SaveTypes current) => new SaveOption
        {
            SaveType = type,
            Label = type.GetDescription(),
            IsSelected = type == current
        };
    }
}

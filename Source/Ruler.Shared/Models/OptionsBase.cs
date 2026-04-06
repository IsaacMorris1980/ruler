namespace Ruler.Shared
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
    }
    public class OpacityOption : OptionBase
    {
        public OpacityOption(double opacity)
        {
            Value = opacity;
            Label = $"{(int)(opacity * 100)}%";
        }
    }

    public class UnitOption : OptionBase
    {
        public MeasurementUnit Unit { get; set; }

        public UnitOption(MeasurementUnit unit)
        {
            Unit = unit;
            Label = unit.GetDescription();
        }
    }

    public class SaveOption : OptionBase
    {
        public SaveTypes SaveType { get; set; }

        public SaveOption(SaveTypes type)
        {
                    SaveType = type;
            Label = type.GetDescription();
        }
    }
}

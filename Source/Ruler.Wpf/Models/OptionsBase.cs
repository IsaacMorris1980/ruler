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
        private bool _isSelected;

        
        public double Value { get; set; }

        
        public string Label { get; set; }

        
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
        
       
    }
    public class ScaleOption : OptionBase { }

    public class OpacityOption : OptionBase { }
    public class UnitOption : OptionBase
    {
        /// <summary>
        /// Provides a strongly-typed reference to the measurement unit.
        /// </summary>
        public MeasurementUnit Unit { get; set; }
    }

    /// <summary>
    /// Concrete option for saving configuration settings, using the SaveType enum.
    /// </summary>
    public class SaveOption : OptionBase
    {
        /// <summary>
        /// Provides a strongly-typed reference to the save configuration type.
        /// </summary>
        public SaveTypes SaveType { get; set; }
    }
}

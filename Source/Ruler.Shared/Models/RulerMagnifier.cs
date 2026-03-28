using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Models
{
    /// <summary>
    /// Represents the persistent state of the Magnifier.
    /// </summary>
    public class RulerMagnifier : ModelBase
    {
        private bool _isEnabled = false;
        private double _scale = 2.0;
        private double _size = 100.0;
        private bool _isSelected = false;

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        public double Scale
        {
            get => _scale;
            set => SetProperty(ref _scale, value);
        }

        public double Size
        {
            get => _size;
            set => SetProperty(ref _size, value);
        }
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Ruler.Shared
{
    /// <summary>
    /// Represents the persistent state of a Guideline.
    /// </summary>
    public class RulerGuideline : ModelBase
    {
        private double _position = 0.0;
        private bool _isVisible = false;
        private bool _isLocked = false;
        private string _color;

        public double Position
        {
            get => _position;
            set => SetProperty(ref _position, value);
        }

        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        public bool IsLocked
        {
            get => _isLocked;
            set => SetProperty(ref _isLocked, value);
        }

        public string Color
        {
            get => _color;
            set => SetProperty(ref _color, value);
        }
    }
}

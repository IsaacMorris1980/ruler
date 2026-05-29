using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.ViewModels
{
    public class CalibrationViewModel : INotifyPropertyChanged
    {
        // ISO/IEC 7810 ID-1 standard dimensions in millimeters
        public const double RealCardWidthMm = 85.60;
        public const double RealCardHeightMm = 53.98;

        private double _cardWidth;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the target layout width of the virtual credit card on screen.
        /// </summary>
        public double CardWidth
        {
            get => _cardWidth;
            set
            {
                // Using the exact floating-point evaluation logic we established
                if (_cardWidth != value)
                {
                    _cardWidth = value;
                    OnPropertyChanged();

                    // Crucial: Tell WPF to re-evaluate the dependent height calculations
                    OnPropertyChanged(nameof(CardHeight));

                    // Also alert any elements tracking the raw math output updates
                    OnPropertyChanged(nameof(CalculatedPixelsPerMm));
                }
            }
        }

        /// <summary>
        /// Gets the calculated layout height, preserving the strict 1.585:1 aspect ratio of a standard card.
        /// </summary>
        public double CardHeight => (_cardWidth * RealCardHeightMm) / RealCardWidthMm;

        /// <summary>
        /// Gets the final calculated Pixels Per Millimeter calculation based on the user's calibration scale adjustments.
        /// </summary>
        public double CalculatedPixelsPerMm => _cardWidth / RealCardWidthMm;

        /// <summary>
        /// Initializes a new instance of the calibration logic engine.
        /// </summary>
        /// <param name="initialPixelWidth">The starting layout tracking size for the UI view (e.g. 320).</param>
        public CalibrationViewModel(double initialPixelWidth)
        {
            _cardWidth = initialPixelWidth;
        }

        /// <summary>
        /// Modern safe property assignment notification utilizing CallerMemberName shortcuts.
        /// </summary>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

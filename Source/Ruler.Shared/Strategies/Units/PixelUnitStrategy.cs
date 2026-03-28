using Ruler.Wpf.Enums;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ruler.Shared.Interfaces;
using Ruler.Shared.Enums;
namespace Ruler.Shared.Strategies.Units
{
    /// <summary>
    /// Defines the specific logic for the screen Pixel unit (using DIPs).
    /// </summary>
    public class PixelUnitStrategy : IUnitStrategy
    {
        public MeasurementUnit UnitType => MeasurementUnit.Pixels;
        public static double SystemScaleFactor { get; set; } = 1.0;

        // Uses 10 subdivisions per 100 DIPs major unit.
        public int Subdivisions => 100;
        // Tick lengths are defined here
        private const double MajorTickLength = 16.0; // At 100px
        private const double HalfTickLength = 10.0; // At 50px
        private const double TenPixelLength = 6.0; // At 10px increments
        private const double FivePixelLength = 4.0; // At 5px increments
        private const double TwoPixelLength = 3.0; // At 2px increments
        private const double OnePixelLength = 2.0; // At 1px increments
        public double GetMajorUnitDip(double dipPerInch)
        {
            if (SystemScaleFactor <= 0) return MajorUnitUncompensated;

            // 100 Pixels in DIPs (100 DIPs) divided by the scale factor.
            return MajorUnitUncompensated / SystemScaleFactor;
        }

        public string GetLabelText(int majorIndex)
        {
            // Labels should display the actual DIP count: 100px, 200px, 300px, etc.
            return $"{majorIndex * (int)MajorUnitUncompensated}px";
        }
        /// <summary>
        /// Returns the appropriate tick length for the given subdivision index (0 to 99).
        /// Index 'j' represents the j-th pixel position within the 100-DIP unit.
        /// </summary>
        public double GetTickLength(double currentPosition, double dipPerInch, double rulerDepth)
        {
            double uncompensatedPositionDip = currentPosition * SystemScaleFactor;
            int subdivisionIndex = (int)Math.Round(uncompensatedPositionDip) % (int)MajorUnitUncompensated;

            // Index 0 is the Major Tick (start of the unit)
            if (subdivisionIndex == 0)
            {
                return MajorTickLength;
            }

            // Check for half mark (50px)
            if (subdivisionIndex == 50)
            {
                return HalfTickLength;
            }

            // Check for 10px marks (10, 20, 30, 40, 60, 70, 80, 90)
            if (subdivisionIndex % 10 == 0)
            {
                return TenPixelLength;
            }

            // Check for 5px marks (5, 15, 25, 35, 45, 55, 65, 75, 85, 95)
            // Note: 50 is caught by HalfTickLength
            if (subdivisionIndex % 5 == 0)
            {
                return FivePixelLength;
            }

            // Check for 2px marks (2, 4, 6, 8, 12, 14, ...)
           return TwoPixelLength;
          

        }
        public double GetSmallestUnitDip(double dipPerInch)
        {
            if (SystemScaleFactor <= 0) return 2.0;

            // 1 Pixel (1 DIP / dipPerInch) is compensated by dividing by the scale factor.
            return (2.0 / SystemScaleFactor);
        }
        private const double MajorUnitUncompensated = 100.0;
        public double MajorTickLengthDip(double rulerDepth)
        {
            // Returns a fixed length (16.0 DIPs), ignoring rulerDepth because ticks are not based on percentage.
            return MajorTickLength;
        }
    }
}

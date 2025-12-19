using Ruler.Wpf.Enums;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Wpf.Services.Persistence.Strategy.Units
{
    /// <summary>
    /// Defines the specific logic for the Imperial Inch unit.
    /// </summary>
    public class InchUnitStrategy : IUnitStrategy
    {
        public enum TickScaleMode { Fixed, Proportional }
        public static TickScaleMode CurrentScaleMode { get; set; } = TickScaleMode.Fixed;
        public MeasurementUnit UnitType => MeasurementUnit.Inches;       
        public int Subdivisions => 16; // Standard 1/16th inch markings

        // The tick lengths are now defined here, specific to the Inches unit.
        private const double MajorTickLength = 16.0;
        private const double HalfTickLength = 10.0; // 1/2 inch mark (index 8)
        private const double QuarterTickLength = 6.0; // 1/4 inch mark (index 4, 12)
        private const double EighthTickLength = 6.0; // NEW: At 1/8 inch
        private const double SubTickLength = 3.0; // At 1/16 inch

        private const double MajorUnitDip = 96.0;
        public double MajorTickLengthDip(double rulerDepth)
        {
            return MajorTickLength;
        }
        public double GetMajorUnitDip(double dipPerInch)
        {
            // 1 inch = 96 DIPs
            return MajorUnitDip;
        }
        public double GetSmallestUnitDip(double dipPerInch)
        {
            return MajorUnitDip / 16.0;
        }

        public string GetLabelText(int majorIndex)
        {
            string mode = CurrentScaleMode == TickScaleMode.Fixed ? "Real-World" : "Scaled";
            return $"{majorIndex} in";
        }
        /// <summary>
        /// Returns the appropriate tick length for the given subdivision index (0 is major, 1-15 are minor).
        /// </summary>
        public double GetTickLength(double currentPosition, double dipPerInch, double rulerDepth)
        {
            double subdivisionDip = Math.Round(currentPosition % MajorUnitDip);
            double sixteenths = subdivisionDip / (MajorUnitDip / 16.0);
            int roundedSixteenths = (int)Math.Round(sixteenths);

            // --- Tick Determination Logic (fixed lengths) ---
            if (roundedSixteenths == 0 || roundedSixteenths == 16) return MajorTickLength;
            if (roundedSixteenths % 8 == 0) return HalfTickLength;
            if (roundedSixteenths % 4 == 0) return QuarterTickLength;
            if (roundedSixteenths % 2 == 0) return EighthTickLength;

            return SubTickLength;
        }
    }
}

using Ruler.Shared.Enums;
using Ruler.Shared.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Strategies.Units
{
    /// <summary>
    /// Defines the specific logic for the Metric Centimeter unit.
    /// </summary>
    public class CentimeterUnitStrategy : IUnitStrategy
    {
        public enum TickScaleMode { Fixed, Proportional }
        public static TickScaleMode CurrentScaleMode { get; set; } = TickScaleMode.Fixed;
        public MeasurementUnit UnitType => MeasurementUnit.Centimeters;
        private const double MajorUnitDip = 96.0 / 2.54;
        // Metric uses 10 subdivisions per major unit (1 cm)
        public int Subdivisions => 10;
        // The tick lengths are now defined here, specific to the Centimeters unit.
        private const double MajorTickLength = 16.0;
        private const double HalfTickLength = 10.0; // 0.5 cm mark (index 5)
        private const double SubTickLength = 3.0; // 1 mm mark
        public double GetSmallestUnitDip(double dipPerInch)
        {
            // 1 mm in DIPs
            return MajorUnitDip / 10.0;
        }
        public double GetMajorUnitDip(double dipPerInch)
        {
            // 1 inch = 2.54 cm. DIPs per cm = DIPs per inch / 2.54
            return MajorUnitDip;
        }

        public string GetLabelText(int majorIndex)
        {
            string mode = CurrentScaleMode == TickScaleMode.Fixed ? "Real-World" : "Scaled";
            return $"{majorIndex} cm";
        }
        public double MajorTickLengthDip(double rulerDepth)
        {
            return MajorTickLength;
        }
        public double GetTickLength(double currentPosition, double dipPerInch, double rulerDepth)
        {
            double subdivisionDip = currentPosition % MajorUnitDip;

            // Calculate subdivision level based on millimeters (10 mm per cm)
            int roundedMillimeters = (int)Math.Round(subdivisionDip/(MajorUnitDip/10.0));

            // --- Tick Determination Logic (fixed lengths) ---
            if (roundedMillimeters == 0 || roundedMillimeters == 10) return MajorTickLength;

            // 5 mm mark
            if (roundedMillimeters == 5) return HalfTickLength;

            // 1 mm marks
            return SubTickLength;
        }
    }
}

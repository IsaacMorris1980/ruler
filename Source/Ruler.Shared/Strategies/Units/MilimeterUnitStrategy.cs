
using Ruler.Wpf.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ruler.Shared.Interfaces;
using Ruler.Shared.Enums;
using Ruler.Shared.Models;

namespace Ruler.Shared.Strategies.Units
{
    /// <summary>
    /// Defines the specific logic for the Metric Millimeter unit.
    /// Note: The major unit is treated as 1cm (10mm) for labeling.
    /// </summary>
    public class MillimeterUnitStrategy : OptionBase, IUnitStrategy
    {
        public MillimeterUnitStrategy() { }
        public MillimeterUnitStrategy(string label) 
        {
            this.Label = label; 
        }
        public MeasurementUnit UnitType => MeasurementUnit.Millimeters;
        public int enumInt => (int)MeasurementUnit.Millimeters;
        public static TickScaleMode CurrentScaleMode { get; set; } = TickScaleMode.Fixed;
        private const double MajorUnitDip = 96.0 / 2.54;

        // Metric uses 10 subdivisions per major unit (1 cm)
        public int Subdivisions => 10;
        // Tick lengths are defined here
        private const double MajorTickLength = 16.0; // At 10mm (1cm)
        private const double HalfTickLength = 10.0; // At 5mm
        private const double SubTickLength = 3.0; // At 1mm
        public double GetSmallestUnitDip(double dipPerInch)
        {
            // 1 mm in DIPs
            return MajorUnitDip / 10.0;
        }
        public double GetMajorUnitDip(double dipPerInch)
        {
            // 1 inch = 25.4 mm. DIPs per cm (10mm) = (DIPs per inch * 10.0) / 25.4
            return MajorUnitDip;
        }
        public double MajorTickLengthDip(double rulerDepth)
        {
            return MajorTickLength;
        }

        public string GetLabelText(int majorIndex)
        {
            // Label is in mm, reflecting the number of mm from the start: 10mm, 20mm, 30mm, etc.
            return $"{majorIndex * 10}mm";
        }
        /// <summary>
        /// Returns the appropriate tick length for the given subdivision index (0 is major, 1-9 are minor).
        /// </summary>
        public double GetTickLength(double currentPosition, double dipPerInch, double rulerDepth)
        {
            double subdivisionDip = currentPosition % MajorUnitDip;

            // Calculate subdivision level based on millimeters (10 mm per cm)
            int roundedMillimeters = (int)Math.Round(subdivisionDip / (MajorUnitDip / 10.0));        

            // --- Tick Determination Logic (fixed lengths) ---
            if (roundedMillimeters == 0 || roundedMillimeters == 10) return MajorTickLength;

            // 5 mm mark
            if (roundedMillimeters == 5) return HalfTickLength;

            // 1 mm marks
            return SubTickLength;
        }
    }
}

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
    /// Defines the specific logic for the DTP Point unit.
    /// </summary>
    public class PointUnitStrategy : IUnitStrategy
    {
        public static double SystemScaleFactor { get; set; } = 1.0;
        public MeasurementUnit UnitType => MeasurementUnit.Points;
        private const double MajorUnitUncompensated = 72.0;

        public double GetSmallestUnitDip(double dipPerInch)
        {
            if (SystemScaleFactor <= 0) return 1.0;

            // 1 Point in DIPs (dipPerInch / 72.0) divided by the scale factor.
            return (dipPerInch / 72.0) / SystemScaleFactor;
        }

        // Imperial/DTP units often use 8 subdivisions (1/8 inch/pica/point marks)
        public int Subdivisions => 6;
        // Tick lengths are defined here
        private const double MajorTickLength = 16.0; // At 72 points (1 inch)
        private const double HalfTickLength = 10.0; // At 36 points
        private const double PicaTickLength = 6.0; // At 12 points (1 pica)
        public double GetMajorUnitDip(double dipPerInch)
        {
            if (SystemScaleFactor <= 0) return dipPerInch;

            // 72 Points in DIPs (dipPerInch) divided by the scale factor.
            return (dipPerInch) / SystemScaleFactor;
        }
        public double MajorTickLengthDip(double rulerDepth)
        {
            return MajorTickLength;
        }
        public string GetLabelText(int majorIndex)
        {
            // Label in points, where the major unit is 72 points: 72pt, 144pt, etc.
            return $"{majorIndex * (int)MajorUnitUncompensated}pt";
        }
        /// <summary>
        /// Returns the appropriate tick length for the given subdivision index (0 is major, 1-5 are minor).
        /// </summary>
        public double GetTickLength(double currentPosition, double dipPerInch, double rulerDepth)
        {
            // Calculate subdivision based on the total number of points in the major unit (72 points).
            double uncompensatedPositionDip = currentPosition * SystemScaleFactor;

            // Convert DIP position to Points (72 points per dipPerInch)
            double pointsPerDip = 72.0 / dipPerInch;
            int subdivisionPoint = (int)Math.Round(uncompensatedPositionDip * pointsPerDip) % (int)MajorUnitUncompensated;

            // --- Tick Determination Logic (fixed lengths) ---
            if (subdivisionPoint == 0) return MajorTickLength;
            if (subdivisionPoint == 36) return HalfTickLength;
           
            return PicaTickLength;
        }
    }
}

using Ruler.Wpf.Enums;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Wpf.Common
{
    public class UniversalScale
    {
        public bool IsPhysicalMode { get; set; } // Switches between Effective and Raw DPI
        public double UnitFactor { get; set; }   // 25.4 for mm, 1.0 for inches, etc.
        public double CalibrationOffset { get; set; } = 1.0; // From MonitorProfile
        public MeasurementUnit UnitType { get; set; } // For reference, not used in calculations

        public double ToUnits(double pixels, double effectiveDpi, double rawDpi)
        {
            // 1. Pick the OS scaling (Effective) or the hardware scaling (Raw)
            double baseDpi = IsPhysicalMode ? rawDpi : effectiveDpi;

            // 2. Apply calibration to the baseline DPI
            // This corrects the "Lies" monitors tell about their physical size
            double calibratedDpi = baseDpi * CalibrationOffset;

            // 3. (Pixels / DPI) = Inches -> Inches * UnitFactor = Final Measurement
            return (pixels / calibratedDpi) * UnitFactor;
        }
        public double ToPixels(double units, double effectiveDpi, double rawDpi)
        {
            // 1. Pick the base DPI density (Effective vs. Raw)
            double baseDpi = IsPhysicalMode ? rawDpi : effectiveDpi;

            // 2. Apply calibration to the baseline DPI
            double calibratedDpi = baseDpi * CalibrationOffset;

            // 3. (Units / UnitFactor) = Inches -> Inches * CalibratedDpi = Pixels
            return (units / UnitFactor) * calibratedDpi;
        }
    }
}

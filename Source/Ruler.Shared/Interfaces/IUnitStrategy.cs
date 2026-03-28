using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ruler.Wpf.Enums;
using Ruler.Shared.Enums;

namespace Ruler.Shared.Interfaces
{
    /// <summary>
    /// Defines the contract for all measurement unit strategies.
    /// Each concrete strategy class (e.g., InchUnitStrategy, CMUnitStrategy)
    /// will implement this interface, encapsulating all unit-specific logic.
    /// </summary>
    public interface IUnitStrategy
    {
        /// <summary>
        /// Gets the type of measurement unit this strategy handles.
        /// </summary>
        MeasurementUnit UnitType { get; }

        /// <summary>
        /// Gets the number of minor tick subdivisions per major unit (e.g., 10 for CM, 8 for Inches).
        /// </summary>
        int Subdivisions { get; }

        /// <summary>
        /// Gets the size of one major unit (e.g., 1 inch, 1 cm) in Device Independent Pixels (DIPs).
        /// </summary>
        /// <param name="dipPerInch">The constant 96.0 DIPs per inch.</param>
        /// <returns>The major unit size in DIPs.</returns>
        double GetMajorUnitDip(double dipPerInch);

        /// <summary>
        /// Generates the text label for a major tick mark.
        /// </summary>
        /// <param name="majorIndex">The whole number unit index (1, 2, 3, etc.).</param>
        /// <returns>The formatted label text (e.g., "3\"", "5cm").</returns>
        string GetLabelText(int majorIndex);

        /// <summary>
        /// Returns the length of a tick mark for a given subdivision index.
        /// Index 0 = Major Tick (labeled).
        /// Index 1 to Subdivisions-1 = Minor Ticks.
        /// </summary>
        /// <param name="subdivisionIndex">The index of the tick within one major unit (0 to Subdivisions - 1).</param>
        /// <returns>The tick length in Device Independent Pixels (DIPs).</returns>
        double GetTickLength(double currentPosition, double dipPerInch, double rulerDepth);
        double GetSmallestUnitDip(double dipPerInch);
        double MajorTickLengthDip(double rulerDepth);
    }
}

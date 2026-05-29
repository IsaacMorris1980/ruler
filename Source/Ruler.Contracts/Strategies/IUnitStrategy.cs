using Ruler.Contracts.Enums;
using Ruler.Contracts.Models;
namespace Ruler.Contracts.Strategies
{
    /// <summary>
    /// Defines the contract for all measurement unit strategies.
    /// Each concrete strategy class (e.g., InchUnitStrategy, CMUnitStrategy)
    /// will implement this interface, encapsulating all unit-specific logic.
    /// </summary>
    /// <summary>
    /// Defines the contract for all measurement unit strategies, encapsulating both 
    /// unit conversion math and rendering layouts for tick marks.
    /// </summary>
    public interface IUnitStrategy
    {
        /// <summary>
        /// Gets the strongly-typed measurement unit type for this strategy.
        /// </summary>
        MeasurementUnit UnitType { get; }

        /// <summary>
        /// Gets the number of minor tick subdivisions per major unit (e.g., 10 for CM, 8 for Inches).
        /// </summary>
        int Subdivisions { get; }

        #region Value Conversion Methods

        /// <summary>
        /// Translates a raw WPF layout DIP size back into the user-facing unit value.
        /// </summary>
        double FromPixels(double wpfDips, double scaleFactor, double hardwarePmm);

        /// <summary>
        /// Translates a user input value into a WPF-compatible layout DIP size.
        /// </summary>
        double ToPixels(double value, double scaleFactor, double hardwarePmm);

        #endregion

        #region UI Rendering Methods

        /// <summary>
        /// Gets the size of one major unit (e.g., 1 inch, 1 cm) in Device Independent Pixels (DIPs).
        /// </summary>
        double GetMajorUnitDip(double scaleFactor, double hardwarePmm);

        /// <summary>
        /// Gets the size of the smallest tick interval division in Device Independent Pixels (DIPs).
        /// </summary>
        double GetSmallestUnitDip(double scaleFactor, double hardwarePmm);

        /// <summary>
        /// Generates the text label for a major tick mark (e.g., "3\"", "5 cm").
        /// </summary>
        string GetLabelText(int majorIndex);

        /// <summary>
        /// Returns the visual height/length of a tick mark line for a given subdivision index position.
        /// </summary>
        double GetTickLength(int subdivisionIndex, double rulerDepth);

        /// <summary>
        /// Gets the full layout length of a primary whole major tick mark line.
        /// </summary>
        double MajorTickLengthDip(double rulerDepth);
        /// <summary>
        /// Gets the localized string abbreviation (e.g., "px", "pt", "in", "mm").
        /// </summary>
        string Abbreviation { get; }

        #endregion
    }
}

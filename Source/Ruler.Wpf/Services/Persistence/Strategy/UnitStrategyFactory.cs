using Ruler.Wpf.Enums;
using Ruler.Wpf.Services.Persistence.Strategy.Units;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Wpf.Services.Persistence.Strategy
{
    /// <summary>
    /// A simple factory to create the correct IUnitStrategy implementation
    /// based on the requested MeasurementUnit enum value.
    /// </summary>
    public static class UnitStrategyFactory
    {
        private static readonly Dictionary<MeasurementUnit, IUnitStrategy> Strategies = new Dictionary<MeasurementUnit, IUnitStrategy>();

        static UnitStrategyFactory()
        {
            // Register all known unit strategies (Now using dedicated class files)
            Strategies.Add(MeasurementUnit.Inches, new InchUnitStrategy());
            Strategies.Add(MeasurementUnit.Centimeters, new CentimeterUnitStrategy());
            Strategies.Add(MeasurementUnit.Millimeters, new MillimeterUnitStrategy());
            Strategies.Add(MeasurementUnit.Pixels, new PixelUnitStrategy());
            Strategies.Add(MeasurementUnit.Points, new PointUnitStrategy());          
        }

        public static IUnitStrategy GetStrategy(MeasurementUnit unit)
        {
            // Check cache first
            if (Strategies.ContainsKey(unit))
            {
                return Strategies[unit];
            }

            IUnitStrategy strategy = null;

            // Use a traditional switch statement
            switch (unit)
            {
                case MeasurementUnit.Inches:
                    strategy = new InchUnitStrategy();
                    break;
                case MeasurementUnit.Millimeters:
                    strategy = new MillimeterUnitStrategy();
                    break;
                case MeasurementUnit.Centimeters:
                    strategy = new CentimeterUnitStrategy();
                    break;
                case MeasurementUnit.Pixels:
                    strategy = new PixelUnitStrategy();
                    break;
                case MeasurementUnit.Points:
                    strategy = new PointUnitStrategy();
                    break;               
                // Default fallback
                default:
                    strategy = new PixelUnitStrategy();
                    break;
            }

            // Cache and return the new strategy instance
            Strategies[unit] = strategy;
            return strategy;
        }
    }

    // The placeholder classes have been moved to their own files and removed from here.
}

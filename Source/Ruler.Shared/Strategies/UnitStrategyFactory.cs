using Ruler.Wpf.Enums;
using Ruler.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ruler.Shared.Enums;
using Ruler.Shared.Strategies.Units;

namespace Ruler.Shared.Strategies
{
    public class UnitStrategyFactory
    {
        private static readonly Dictionary<MeasurementUnit, IUnitStrategy> _strategies =
            new Dictionary<MeasurementUnit, IUnitStrategy>
        {
            { MeasurementUnit.Inches, new InchUnitStrategy() },
            { MeasurementUnit.Centimeters, new CentimeterUnitStrategy() },
            { MeasurementUnit.Millimeters, new MillimeterUnitStrategy() },
            { MeasurementUnit.Points, new PointUnitStrategy() },
            { MeasurementUnit.Pixels, new PixelUnitStrategy() }
        };

        public static IUnitStrategy GetStrategy(MeasurementUnit unit)
        {
            if (_strategies.ContainsKey(unit))
            {
                return _strategies[unit];
            }

            throw new ArgumentException("Unit " + unit + " is not supported.");
        }
    
    }
}

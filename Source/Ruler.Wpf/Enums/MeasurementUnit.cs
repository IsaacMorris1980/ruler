using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Wpf.Enums
{
    public enum MeasurementUnit
    {
        /// <summary>Unit is Device Independent Pixels (DIPs). 1 DIP = 1/96th inch.</summary>
        DIPs = 0,

        /// <summary>Unit is Pixels (usually 1:1 with DIPs in standard WPF context).</summary>
        Pixels = 1,

        /// <summary>Unit is Inches. 1 inch = 96 DIPs.</summary>
        Inches = 2,

        /// <summary>Unit is Centimeters. 1 cm = ~37.795 DIPs.</summary>
        Centimeters = 3,

        /// <summary>Unit is Millimeters. 1 mm = ~3.7795 DIPs.</summary>
        Millimeters = 4,
        /// <summary>Unit is Points (typographic unit). 1 point = ~1.333 DIPs.</summary>
        Points = 5     
    }
}

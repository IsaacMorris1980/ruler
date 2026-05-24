using System.ComponentModel;

namespace Ruler.Contracts.Enums
{
    public enum MeasurementUnit
    {
        [Description("Device Independant Pixels (DIPs)")]      
        /// <summary>Unit is Device Independent Pixels (DIPs). 1 DIP = 1/96th inch.</summary>
        DIPs = 0,
        [Description("Pixels (px)")]
        /// <summary>Unit is Pixels (usually 1:1 with DIPs in standard WPF context).</summary>
        Pixels = 1,
        [Description("Inches (in)")]
        /// <summary>Unit is Inches. 1 inch = 96 DIPs.</summary>
        Inches = 2,
        [Description("Centimeters (cm)")]
        /// <summary>Unit is Centimeters. 1 cm = ~37.795 DIPs.</summary>
        Centimeters = 3,
        [Description("Milimeters (mm)")]
        /// <summary>Unit is Millimeters. 1 mm = ~3.7795 DIPs.</summary>
        Millimeters = 4,
        [Description("Points (pt)")]
        /// <summary>Unit is Points (typographic unit). 1 point = ~1.333 DIPs.</summary>
        Points = 5     
    }
}

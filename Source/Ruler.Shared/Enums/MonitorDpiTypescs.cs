using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared
{
    public enum MonitorDpiType
    {
        EffectiveDpi = 0, // Includes OS scaling (125%, 150%, etc.)
        AngularDpi = 1,   // Based on viewing angle
        RawDpi = 2        // Literal hardware pixels per inch
    }
}

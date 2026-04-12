using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared
{
    public enum SaveTypes
    {
        [Description("Save Nothing")]
        none,
        [Description("Save Everything")]
        all,
        [Description("Save Location Only")]
        location,
        [Description("Save Size Only")]
        size,
        [Description("Save Appearance Only")]
        appearance
    }
}

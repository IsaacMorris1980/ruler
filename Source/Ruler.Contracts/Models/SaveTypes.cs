using System.ComponentModel;

namespace Ruler.Contracts.Models
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

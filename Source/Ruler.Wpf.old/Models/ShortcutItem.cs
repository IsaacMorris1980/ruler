using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Wpf.Models
{
    public class ShortcutItem
    {
        public string Keys { get; set; }       // e.g., "Ctrl + R"
        public string Description { get; set; } // e.g., "Reset all rulers to default"
        public string Category { get; set; }    // e.g., "Reset / Actions"
    }
}

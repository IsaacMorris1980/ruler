using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Wpf.Models
{
    public class RulerScale
    {
        public string DisplayName { get; set; } 
        public double ScaleFactor { get; set; }
        public RulerScale(string displayName, double scaleFactor)
        {
            DisplayName = displayName;
            ScaleFactor = scaleFactor;
        }
    }
}

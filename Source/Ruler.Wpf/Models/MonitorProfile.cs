using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Wpf.Models
{
    /// <summary>
    /// Represents a specific hardware monitor configuration. 
    /// Rulers use the MonitorId to determine which setup they belong to.
    /// </summary>
    public class MonitorProfile
    {
        public string MonitorId { get; set; }
        public string MonitorName { get; set; }
        public DateTime LastUsed { get; set; }

        /// <summary>
        /// The dots per inch of the display. 96 is standard (100%).
        /// </summary>
        public double MonitorDpi { get; set; }

        /// <summary>
        /// The OS scaling factor (e.g., 1.25 for 125%).
        /// </summary>
        public double ScaleFactor { get; set; }
        public double calibratedScaleFactor { get; set; }

        /// <summary>
        /// Indicates if the monitor is identified as a Generic PnP device
        /// or if hardware-specific calibration is unavailable.
        /// </summary>
        public bool IsGeneric { get; set; }

        public MonitorProfile() { }

        public MonitorProfile(string monitorId, string monitorName, double dpi, double scale, bool isGeneric)
        {
            MonitorId = monitorId;
            MonitorName = monitorName;
            MonitorDpi = dpi;
            ScaleFactor = scale;
            IsGeneric = isGeneric;
            LastUsed = DateTime.Now;
        }
    }
}

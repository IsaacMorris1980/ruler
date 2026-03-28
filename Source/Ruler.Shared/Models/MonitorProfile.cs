using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Models
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
        public bool IsActive { get; set; }=false;

        /// <summary>
        /// The dots per inch of the display. 96 is standard (100%).
        /// </summary>
        public double HardwareDpi { get; set; }

        /// <summary>
        /// The OS scaling factor (e.g., 1.25 for 125%).
        /// </summary>
        public double OSDpi { get; set; }
        public double CalibratedDpi { get; set; }
        public  bool IsCalibrated { get; set; } =false;

        /// <summary>
        /// Indicates if the monitor is identified as a Generic PnP device
        /// or if hardware-specific calibration is unavailable.
        /// </summary>
        public bool IsGeneric { get; set; }

        public MonitorProfile() { }

        public MonitorProfile(string monitorId, string monitorName, double dpi, double scale, bool isGeneric,double calibratedDpi=0.0)
        {
            MonitorId = monitorId;
            MonitorName = monitorName;
            HardwareDpi = dpi;
            OSDpi = scale;
            IsGeneric = isGeneric;
            IsCalibrated=!isGeneric;
            CalibratedDpi = calibratedDpi ;
            LastUsed = DateTime.Now;
        }
    }
}

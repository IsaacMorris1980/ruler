using Ruler.Contracts.Models;
using System;
using System.Collections.Generic;

namespace Ruler.Contracts.Services.OS
{
    public interface IHardwareMonitorService
    {
        /// <summary>
        /// Returns a collection of stable device strings for active desktop monitors.
        /// </summary>
        List<MonitorProfile> GetActiveHardwareProfiles();

        /// <summary>
        /// Retrieves the real-time unmanaged pixel density for a given window handle.
        /// </summary>
        double GetEffectiveDpi(IntPtr windowHandle);

        /// <summary>
        /// Identifies the HMONITOR pointer coordinate container bounding a window handle.
        /// </summary>
        IntPtr GetMonitorHandle(IntPtr windowHandle);

        /// <summary>
        /// Generates a unique system display handle identification key.
        /// </summary>
        string GetHardwareId(IntPtr windowHandle);
    }
}
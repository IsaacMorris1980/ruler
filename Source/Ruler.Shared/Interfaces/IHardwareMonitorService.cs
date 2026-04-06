using System;
using System.Collections.Generic;
namespace Ruler.Shared
{
    public interface IHardwareMonitorService
    {
        List<string> GetActiveHardwareProfiles();
        double GetDpiScale(IntPtr windowHandle);
        IntPtr GetMonitorHandleFromWindow(IntPtr windowHandle);
    }
}
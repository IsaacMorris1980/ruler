using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ruler.Wpf.Models;

namespace Ruler.Shared.Interfaces
{
    public interface IHardwareMonitorService
    {
        List<MonitorProfile> GetActiveHardwareProfiles();
        double GetDpiScale(IntPtr windowHandle);
        IntPtr GetMonitorHandleFromWindow(IntPtr windowHandle);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using static Ruler.Wpf.Common.NativeMethods;

namespace Ruler.Wpf.Services
{
    public interface IWindowPlacementService
    {
        void EnsureVisibility(Window window);
        void SpanAllMonitors(Window window);
        MonitorPositionData GetMonitorRelativePosition(Window window);
        Point GetVirtualPosition(MonitorPositionData savedData);
    }
}

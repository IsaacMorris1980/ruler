using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static Ruler.Shared.Common.NativeStructures;

using static Ruler.Wpf.Common.NativeMethods;

namespace Ruler.Shared.Interfaces
{
    public interface IWindowPlacementService
    {
        void EnsureVisibility(Window window);
        void SpanAllMonitors(Window window);
        MonitorPositionData GetMonitorRelativePosition(Window window);
        POINT GetVirtualPosition(MonitorPositionData savedData);
    }
}

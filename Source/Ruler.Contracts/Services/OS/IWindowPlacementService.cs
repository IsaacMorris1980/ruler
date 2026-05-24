using System;

using Ruler.Contracts.Interop;

namespace Ruler.Contracts.Services.OS
{
    public interface IWindowPlacementService
    {
        void EnsureVisibility(IntPtr window);
        void SpanAllMonitors(IntPtr window);
        NativeStructures.MonitorPositionData GetMonitorRelativePosition(IntPtr window);
        NativeStructures.POINT GetVirtualPosition(NativeStructures.MonitorPositionData savedData);
    }
}

namespace Ruler.Shared
{
    public interface IWindowPlacementService
    {
        void EnsureVisibility(Window window);
        void SpanAllMonitors(Window window);
        MonitorPositionData GetMonitorRelativePosition(Window window);
        POINT GetVirtualPosition(MonitorPositionData savedData);
    }
}

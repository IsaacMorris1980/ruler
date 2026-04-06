using System.Collections.Generic;

namespace Ruler.Shared
{
    public interface IMonitorProfileManager
    {
        void Initialize();
        void Save();
        MonitorProfile GetProfile(string deviceId);
        void UpdateProfile(MonitorProfile profile);
        List<MonitorProfile> Profiles { get; }
    }
}

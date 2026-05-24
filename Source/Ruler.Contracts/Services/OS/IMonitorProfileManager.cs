using System.Collections.Generic;

using Ruler.Contracts.Models;

namespace Ruler.Contracts.Services.OS
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

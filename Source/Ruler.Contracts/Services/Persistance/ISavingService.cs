using System.Collections.Generic;
using Ruler.Contracts.Models;

namespace Ruler.Contracts.Services.Persistance
{
    public interface ISavingService
    {
        // Existing Ruler persistence methods
        void Save(List<IRulerInfo> items);
        List<IRulerInfo> Load();

        // New Monitor Profile persistence methods
        void SaveMonitorProfiles(List<MonitorProfile> profiles);
        List<MonitorProfile> LoadMonitorProfiles();
    }
}
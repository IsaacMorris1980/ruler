using Ruler.Wpf.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Interfaces
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

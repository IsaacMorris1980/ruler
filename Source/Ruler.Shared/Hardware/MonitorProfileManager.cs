using Ruler.Wpf.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ruler.Shared.Interfaces;
using Ruler.Shared.Models;

namespace Ruler.Shared.Hardware
{
    public class MonitorProfileManager : IMonitorProfileManager
    {
        private readonly IHardwareMonitorService _hardware;
        private readonly ISavingService _persistence;
        private List<MonitorProfile> _profiles;
        private ILoggingService<MonitorProfileManager> _logger;

        public List<MonitorProfile> Profiles => _profiles ?? new List<MonitorProfile>();

        public MonitorProfileManager(IHardwareMonitorService hardware, ISavingService persistence,ILoggingService<MonitorProfileManager> logger)
        {
            _hardware = hardware;
            _persistence = persistence;
            _logger = logger;
        }

        public void Initialize()
        {
            var saved = _persistence.Load<MonitorProfile>() ?? new List<MonitorProfile>();
            var active = _hardware.GetActiveHardwareProfiles();
            var activeMonitorIds = active
               .Select(p => p.MonitorId)
               .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var  newsaved = saved.Where(r=>activeMonitorIds.Contains(r.MonitorId)).ToList();
           List<MonitorProfile> newactive = active.Where(r=>!newsaved.Any(s=>s.MonitorId==r.MonitorId)).ToList();
             newsaved.AddRange(newactive);
            
            // Synchronize logic

            _profiles = newsaved;
        }

        public MonitorProfile GetProfile(string deviceId)
        {
            return _profiles?.FirstOrDefault(p => p.MonitorId == deviceId)
                   ?? _profiles?.FirstOrDefault(p => p.MonitorId == Screen.PrimaryScreen.DeviceName);
        }

        public void UpdateProfile(MonitorProfile profile)
        {
            var existing = _profiles.FirstOrDefault(m => m.MonitorId == profile.MonitorId);
            if (existing != null)
            {
                existing.MonitorName = profile.MonitorName;
                existing.IsCalibrated = profile.IsCalibrated;
                existing.CalibratedDpi = profile.CalibratedDpi;
                existing.LastUsed = DateTime.Now;
            }
            else
            {
                _profiles.Add(profile);
            }
        }

        public void Save() => _persistence.Save(_profiles);
    }
}

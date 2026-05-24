using System;
using System.Collections.Generic;
using System.Linq;
using Ruler.Contracts.Services.Persistance;
using Ruler.Contracts.Services.OS;

using Ruler.Contracts.Models;
namespace Ruler.Shared
{
    public class MonitorProfileManager : IMonitorProfileManager
    {
        private readonly IHardwareMonitorService _hardware;
        private readonly ISavingService _persistence;
        private List<MonitorProfile> _profiles;
        private readonly ILoggingService<MonitorProfileManager> _logger;
        public List<MonitorProfile> Profiles => _profiles ?? new List<MonitorProfile>();
        public MonitorProfileManager(IHardwareMonitorService hardware, ISavingService persistence, ILoggingService<MonitorProfileManager> logger)
        {
            _hardware = hardware;
            _persistence = persistence;
            _logger = logger;
        }
        public void Initialize()
        {
            // 1. Load existing profiles from disk
            var savedProfiles = _persistence.LoadMonitorProfiles() ?? new List<MonitorProfile>();
            // 2. Get currently connected hardware
            var activeHardware = _hardware.GetActiveHardwareProfiles();
            // 3. Mark all saved profiles as inactive by default
            foreach (var profile in savedProfiles)
            {
                profile.IsActive = false;
            }
            // 4. Update existing or add new active monitors
            foreach (var hardwareProfile in activeHardware)
            {
                var existing = savedProfiles.FirstOrDefault(p =>
                    p.MonitorId.Equals(hardwareProfile.MonitorId, StringComparison.OrdinalIgnoreCase));
                if (existing != null)
                {
                    // Monitor is plugged in. Update transient properties but KEEP calibration data.
                    existing.IsActive = true;
                    existing.MonitorName = hardwareProfile.MonitorName;
                    existing.HardwareDpi = hardwareProfile.HardwareDpi;
                    existing.OSDpi = hardwareProfile.OSDpi;
                    existing.LastUsed = DateTime.Now;
                }
                else
                {
                    // Completely new monitor detected. Add it to the list.
                    hardwareProfile.IsActive = true;
                    savedProfiles.Add(hardwareProfile);
                }
            }
            _profiles = savedProfiles;
        }
        public MonitorProfile GetProfile(string deviceId)
        {
            // 1. Try to find the specific requested monitor
            var requestedProfile = _profiles?.FirstOrDefault(p => p.MonitorId == deviceId);
            if (requestedProfile != null)
            {
                return requestedProfile;
            }
            // 2. Fallback: If not found, safely return the first ACTIVE monitor
            // This replaces the WinForms Screen.PrimaryScreen dependency
            return _profiles?.FirstOrDefault(p => p.IsActive) ?? _profiles?.FirstOrDefault();
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
        public void Save() => _persistence.SaveMonitorProfiles(_profiles);
    }
}

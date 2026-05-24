using Newtonsoft.Json;

using Ruler.Contracts.Models;
using Ruler.Contracts.Services.Persistance;
using Ruler.Shared;


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Wpf.Services
{
    // 4. The Refactored Saving Service using DI
    public class SavingService : ISavingService
    {
        // Settings configuration optimized for runtime interface resolution
        private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented
        };

        #region Ruler Info Persistence
        public void Save(List<IRulerInfo> items)
        {
            string json = JsonConvert.SerializeObject(items, _jsonSettings);
            Properties.Settings.Default.RulerCollectionJson = json;
            Properties.Settings.Default.Save();
        }

        public List<IRulerInfo> Load()
        {
            string json = Properties.Settings.Default.RulerCollectionJson;
            if (string.IsNullOrEmpty(json) || json == "{}")
            {
                var a = RulerFactory.CreateDefault();
                return new List<IRulerInfo>(){ a };
            }

            try
            {
                return JsonConvert.DeserializeObject<List<IRulerInfo>>(json, _jsonSettings)
                       ?? new List<IRulerInfo>();
            }
            catch
            {
                return new List<IRulerInfo>();
            }
        }
        #endregion

        #region Monitor Profiles Persistence
        /// <summary>
        /// Serializes and stores the complete collection of monitor profiles into application settings.
        /// </summary>
        public void SaveMonitorProfiles(List<MonitorProfile> profiles)
        {
            try
            {
                // Concrete list serialization safely bypasses explicit TypeNameHandling requirements
                string json = JsonConvert.SerializeObject(profiles, Formatting.Indented);
                Properties.Settings.Default.MonitorCollectionJson = json;
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                // Fallback exception handling block (or route out to your ILoggingService)
                System.Diagnostics.Debug.WriteLine("Error saving monitor profiles data: " + ex.Message);
            }
        }

        /// <summary>
        /// Restores saved hardware profile records from system settings.
        /// </summary>
        public List<MonitorProfile> LoadMonitorProfiles()
        {
            string json = Properties.Settings.Default.MonitorCollectionJson;
            if (string.IsNullOrEmpty(json) || json == "[]" || json == "{}")
            {
                return new List<MonitorProfile>();
            }

            try
            {
                return JsonConvert.DeserializeObject<List<MonitorProfile>>(json)
                       ?? new List<MonitorProfile>();
            }
            catch
            {
                return new List<MonitorProfile>();
            }
        }
        #endregion
    }
}

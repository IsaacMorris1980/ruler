using Newtonsoft.Json;

using Ruler.Wpf.Models;
using Ruler.Wpf.Properties;
using Ruler.Wpf.Services.Persistence.Strategy;

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
        private readonly IServiceProvider _serviceProvider;

        private readonly IDataPreprocessor<RulerInfo> _rulerPreprocessor;
        private readonly ILoggingService<SavingService> _logger;

        private readonly Dictionary<Type, string> _settingsMap = new()
    {
        { typeof(RulerInfo), "RulerSettingsJson" },
        { typeof(MonitorProfile), "MonitorProfilesJson" }
    };

        // We inject IServiceProvider to resolve processors dynamically
        public SavingService(ILoggingService<SavingService> logger,IDataPreprocessor<RulerInfo> dataPreprocessor)
        {
            _rulerPreprocessor = dataPreprocessor;
            _logger = logger;
        }

        public void Save<T>(List<T> items)
        {
            if (items == null || !items.Any()) return;

            IEnumerable<T> dataToSave = items;

            // DI MAGIC: Try to find a preprocessor for this specific type T
            if (typeof(T) == typeof(RulerInfo))
            {
                _logger.LogInfo("Preprocessing RulerInfo data before saving.");
                dataToSave = (IEnumerable<T>)_rulerPreprocessor.Preprocess(items.Cast<RulerInfo>());
            }

            if (_settingsMap.TryGetValue(typeof(T), out string key))
            {
                string json = JsonConvert.SerializeObject(dataToSave);
                Settings.Default[key] = json;
                Settings.Default.Save();
            }
        }

        public List<T> Load<T>()
        {
            if (_settingsMap.TryGetValue(typeof(T), out string key))
            {
                var json = Settings.Default[key]?.ToString();
                if (string.IsNullOrEmpty(json)) return new List<T>();

                return JsonConvert.DeserializeObject<List<T>>(json) ?? new List<T>();
            }
            return new List<T>();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ruler.Shared.Interfaces;
using Ruler.Shared.Models;

namespace Ruler.Wpf.Services
{
    public class PersistenceService : IPersistanceService
    {
        private readonly string _filePath;
        private readonly IRulerInfoPreprocessor _processor;
        private readonly IRulerSerializer _settingsService;
        private readonly IRulerRegistry _registry;

        public PersistenceService(
            IRulerInfoPreprocessor rulerInfoPreprocessor,
            IRulerSerializer settings,
            IRulerRegistry registry)
        {
            _processor = rulerInfoPreprocessor;
            _settingsService = settings;
            _registry = registry;

            string appDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Ruler");
            Directory.CreateDirectory(appDataDir);
            _filePath = Path.Combine(appDataDir, "rulers.json");
        }

        public List<RulerInfo> LoadAll()
        {
            if (!File.Exists(_filePath))
            {
                return new List<RulerInfo>();
            }

            string json = File.ReadAllText(_filePath);
            return _settingsService.DeserializeRulers(json);
        }

        public void SaveAll(IEnumerable<RulerInfo> rulers)
        {
            var rulersToSave = _processor.Preprocess(rulers);
            string json = _settingsService.SerializeRulers(rulersToSave);

            File.WriteAllText(_filePath, json);
        }

        public void Update(RulerInfo info)
        {
            if (_registry.RulerExists(info.ID))
            {
                _registry.UpdateRulerByID(info);
            }
            SaveAll(_registry.GetActiveRulers().Select(r => r.RulerData).ToList());
        }

        public void Reset()
        {
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }
        }
    }
}

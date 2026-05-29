using Ruler.Shared.Models;
using Ruler.Shared.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Wpf.Infrastructure
{
    public static class RulerSessionManager
    {
        // Maintains the combined historical state of active and closed rulers during this execution
        private static readonly List<RulerInfo> _trackedRulers = new List<RulerInfo>();
        private static readonly RulerInfoPreprocessor _preprocessor = new RulerInfoPreprocessor();

        /// <summary>
        /// Initializes the session with models retrieved from persistence on startup.
        /// </summary>
        public static void Initialize(IEnumerable<RulerInfo> initialRulers)
        {
            _trackedRulers.Clear();
            if (initialRulers != null)
            {
                _trackedRulers.AddRange(initialRulers);
            }
        }

        /// <summary>
        /// Explicitly adds a newly created/duplicated ruler to our track list.
        /// </summary>
        public static void RegisterNewRuler(RulerInfo newModel)
        {
            if (!_trackedRulers.Contains(newModel))
            {
                _trackedRulers.Add(newModel);
            }
        }

        /// <summary>
        /// Fired immediately when a window closes. Preserves the model instance in memory.
        /// </summary>
        public static void RegisterWindowClosed(RulerInfo closedModel)
        {
            if (!_trackedRulers.Contains(closedModel))
            {
                _trackedRulers.Add(closedModel);
            }

            // Optional: Auto-save immediately to settings file whenever a window changes state
            PersistToSettings();
        }

        /// <summary>
        /// Cleanses the collection via the Preprocessor (using each ruler's SaveType) and commits to the Settings object.
        /// </summary>
        public static void PersistToSettings()
        {
            try
            {
                // Your library preprocessor automatically handles individual SaveTypes here!
                // It evaluates each item's SaveType enum to decide if it stays or gets stripped out.
                IEnumerable<RulerInfo> processedData = _preprocessor.Preprocess(_trackedRulers);

                // Serialize filtered items to JSON
                string json = SettingsService.SerializeRulers(processedData);

                // Commit back to your WPF Settings configuration file object
                Ruler.Wpf.Properties.Settings.Default.RulerCollection = json;
                Ruler.Wpf.Properties.Settings.Default.Save();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to persist session settings: {ex.Message}");
            }
        }
    }
}

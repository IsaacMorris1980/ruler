using Ruler.Shared.Models;
using Ruler.Shared.Services;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Wpf.Infrastructure
{
    public  class RulerSessionManager
    {
        // Maintains the combined historical state of active and closed rulers during this execution
        private  readonly List<RulerInfo> _trackedRulers = new List<RulerInfo>();
        private readonly RulerInfoPreprocessor _preprocessor = new RulerInfoPreprocessor();

        /// <summary>
        /// Initializes the session with models retrieved from persistence on startup.
        /// </summary>
        public void Initialize(IEnumerable<RulerInfo> initialRulers)
        {
            _trackedRulers.Clear();
            if (initialRulers != null)
            {
                _trackedRulers.AddRange(initialRulers);
            }
        }
        /// <summary>
        /// Safely loads the saved JSON session data from the local settings configuration.
        /// Intercepts configuration XML structural breakages or JSON string syntax corruption (Scenario 3).
        /// </summary>
        public IEnumerable<RulerInfo> LoadSession()
        {
            string json = string.Empty;

            // --- LAYER 1: Validate .NET Configuration XML Structure ---
            try
            {
                json = Ruler.Wpf.Properties.Settings.Default.RulerCollection;
            }
            catch (ConfigurationErrorsException ex)
            {
                System.Diagnostics.Debug.WriteLine($"XML Configuration file corrupted: {ex.Message}");
                return ResetToFactoryDefaults();
            }

            // Safe fallback if it's a first-time launch or the setting is empty
            if (string.IsNullOrWhiteSpace(json))
            {
                return ResetToFactoryDefaults();
            }

            // --- LAYER 2: Validate the Underling JSON String Structure ---
            try
            {
                // Relying on your shared library's deserialization service wrapper
                IEnumerable<RulerInfo> deserializedData = SettingsService.DeserializeRulers(json);

                if (deserializedData == null)
                {
                    return ResetToFactoryDefaults();
                }

                // If deserialization succeeds, initialize the tracking state and pass it to App.xaml.cs
                var initialRulers = deserializedData.ToList();
                Initialize(initialRulers);
                return initialRulers;
            }
            catch (Exception ex)
            {
                // 🛡️ Handles Scenario 3: Inner JSON corruption (manual typos, broken bracket layout, etc.)
                System.Diagnostics.Debug.WriteLine($"JSON Data payload corrupted: {ex.Message}");
                return ResetToFactoryDefaults();
            }
        }
        /// <summary>
        /// Self-healing fallback helper that forces a clean, empty session out of disk danger.
        /// </summary>
        private IEnumerable<RulerInfo> ResetToFactoryDefaults()
        {
            var emptyDefaults = new List<RulerInfo>();

            // Re-initialize local tracking memory array
            Initialize(emptyDefaults);

            try
            {
                // Wipe out the corrupt setting data by forcing an empty layout structure
                Ruler.Wpf.Properties.Settings.Default.RulerCollection = string.Empty;
                Ruler.Wpf.Properties.Settings.Default.Save();
            }
            catch
            {
                // Absolute safety net if file-system IO operations are entirely locked out
            }

            return emptyDefaults;
        }

        /// <summary>
        /// Explicitly adds a newly created/duplicated ruler to our track list.
        /// </summary>
        public void RegisterNewRuler(RulerInfo newModel)
        {
            if (!_trackedRulers.Contains(newModel))
            {
                _trackedRulers.Add(newModel);
            }
        }

        /// <summary>
        /// Fired immediately when a window closes. Preserves the model instance in memory.
        /// </summary>
        public void RegisterWindowClosed(RulerInfo closedModel)
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
        public void PersistToSettings()
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

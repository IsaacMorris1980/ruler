using Ruler.Forms;
using Ruler.Shared.Factories;
using Ruler.Shared.Models;
using Ruler.Shared.Services;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ruler
{
    public static class PersistenceManager
    {
        private static readonly RulerInfoPreprocessor _preprocessor = new RulerInfoPreprocessor();
        private static List<RulerInfo> currentrulers;
        

        /// <summary>
        /// Gathers all open MainForm instances, cleans their data via the Preprocessor,
        /// and saves the resulting JSON string to the User Settings (exe.config).
        /// </summary>
        public static void SaveAll(IEnumerable<Form> savingrulers)
        {
            try
            {
                if (savingrulers == null) { return; }

                var saverulers = savingrulers
                    .OfType<MainForm>()
                    .Select(f => f.RulerData) 
                    .ToList();
                IEnumerable<RulerInfo> processedData = _preprocessor.Preprocess(saverulers);
                string rawJson = SettingsService.SerializeRulers(processedData);
                string signature = SecurityService.SignWithMachineKey(rawJson);
                var signedPackage = new SignedSettings
                {
                    Data = rawJson,
                    Signature = signature
                };
                string finalPayload = JsonConvert.SerializeObject(signedPackage);

                Properties.Settings.Default.RulerCollection = finalPayload;
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                // Log the error or handle it gracefully so the app can still close
                Console.WriteLine($"Failed to save settings: {ex.Message}");
            }
        }

        /// <summary>
        /// Reads the JSON string from exe.config, converts it back to RulerInfo objects,
        /// and spawns the necessary MainForm instances.
        /// </summary>
        public static IEnumerable<RulerInfo> LoadAll()
        {
            try
            {
                string payload = Properties.Settings.Default.RulerCollection;

                if (string.IsNullOrEmpty(payload))
                {
                    return new List<RulerInfo>(){ RulerFactory.CreateDefault() };
                }

                // 1. Unpack the SignedSettings envelope
                var package = JsonConvert.DeserializeObject<SignedSettings>(payload);

                // 2. Verify the data against the signature
                if (SecurityService.VerifyWithMachineKey(package.Data, package.Signature))
                {
                    // 3. Signature is valid: Deserialize the actual RulerInfo list
                    currentrulers = SettingsService.DeserializeRulers(package.Data);
                }
                else
                {
                    // Signature is INVALID: Data was tampered with or corrupted
                    Console.WriteLine("Security verification failed for saved rulers.");
                    return new List<RulerInfo>() { RulerFactory.CreateDefault() };
                }

                // Ensure we actually have data after deserialization
                return (currentrulers != null && currentrulers.Any())
                       ? currentrulers
                       : new List<RulerInfo>(){ RulerFactory.CreateDefault() };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load settings: {ex.Message}");
                return new List<RulerInfo>(){ RulerFactory.CreateDefault() };
            }
        }
    }
    
}

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
using Newtonsoft.Json;


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

        public static void LoadAndUpgradeConfiguration()
        {
            // 1. Ensure our certificate system is ready for action
            InitializeSecurityFromScratch(); // From Stage 2 (Finds or creates the cert)

            string activeThumb = Properties.Settings.Default.MachineCertThumbprints;
            bool encryptionReady = SecurityService.IsSecurityReady(activeThumb);

            // 2. Read the raw data payload (e.g., your Ruler list or settings string)
            string currentPayload = GetRawDataPayload();
            string currentSignature = GetStoredSignature();

            // SCENARIO A: Brand New Install or Completely Unsigned Legacy Version
            if (string.IsNullOrEmpty(currentSignature))
            {
                Log("Unsigned configuration detected. Assuming legacy version upgrade.");

                if (encryptionReady)
                {
                    Log("Active certificate is ready. Upgrading and signing legacy data...");

                    // Re-save the data, which forces the app to sign it this time
                    SaveAndSignConfiguration(currentPayload, activeThumb);

                    Log("Upgrade complete. Data is now secured with Stage 2 verification.");
                }
                else
                {
                    // Edge case: No signature AND no valid cert could be made/found
                    Log("Warning: Legacy data found, but security layer could not be initialized.");
                    HandleInvalidCertificate();
                }
            }
            // SCENARIO B: The New Way (Data is already signed, so we VERIFY)
            else
            {
                if (encryptionReady && VerifyDataSignature(currentPayload, currentSignature, activeThumb))
                {
                    Log("Configuration verification successful. Signature is valid.");
                    ProcessValidData(currentPayload);
                }
                else
                {
                    Log("CRITICAL: Configuration signature verification failed! Data may be tampered with.");
                    HandleSecurityBreach();
                }
            }
        }
        //public static void SaveRegistry(List<RulerCertMetadata> registry)
        //{
        //    string json = JsonConvert.SerializeObject(registry);
        //    Properties.Settings.Default.CertificateRegistryJson = json;
        //    Properties.Settings.Default.Save();
        //}

        //public static List<RulerCertMetadata> LoadRegistry()
        //{
        //    string json = Properties.Settings.Default.CertificateRegistryJson;
        //    if (string.IsNullOrEmpty(json)) return new List<RulerCertMetadata>();

        //    return JsonConvert.DeserializeObject<List<RulerCertMetadata>>(json);
        //}
    }
    
}

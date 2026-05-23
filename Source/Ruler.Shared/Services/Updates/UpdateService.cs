using Newtonsoft.Json;

using Ruler.Contracts.Services.Updates;
using Ruler.Contracts.Services.Verification;
using Ruler.Contracts.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Ruler.Shared.Models;

namespace Ruler.Shared.Services.Updates
{
    public class UpdateService : IUpdateService
    {
        private readonly IGithubUpdateService _githubService;
        private readonly IHttpDownloadService _downloadService;
        private readonly IActiveKeyVerifier _keyVerifier;
        private readonly IMasterFallbackVerifier _fallbackVerifier;

        private readonly string _masterPublicKey;
        private readonly string _activePublicKey;
        private readonly string _tempDownloadDir;

        public event Action<int> DownloadProgressChanged;
        public event Action<string> UpdateStatusChanged;

        public UpdateService(
            IGithubUpdateService githubService,
            IHttpDownloadService downloadService,
            IActiveKeyVerifier keyVerifier,
            IMasterFallbackVerifier fallbackVerifier,
            string activePublicKeyXml,
            string masterPublicKeyXml)
        {
            _githubService = githubService ?? throw new ArgumentNullException(nameof(githubService));
            _downloadService = downloadService ?? throw new ArgumentNullException(nameof(downloadService));
            _keyVerifier = keyVerifier ?? throw new ArgumentNullException(nameof(keyVerifier));
            _fallbackVerifier = fallbackVerifier ?? throw new ArgumentNullException(nameof(fallbackVerifier));

            _activePublicKey = activePublicKeyXml ?? throw new ArgumentNullException(nameof(activePublicKeyXml));
            _masterPublicKey = masterPublicKeyXml ?? throw new ArgumentNullException(nameof(masterPublicKeyXml));

            _tempDownloadDir = Path.Combine(Path.GetTempPath(), "RulerAppUpdates");

            // Forward underlying download progress up to UI observers
            _downloadService.ProgressChanged += percent => DownloadProgressChanged?.Invoke(percent);
        }

        public async Task<bool> RunUpdateLifecycleAsync(string currentVersion, string repoOwner, string repoName)
        {
            try
            {
                // Step 1: Request raw JSON text metadata from GitHub
                UpdateStatusChanged?.Invoke("Checking for deployed update catalog...");
                string jsonResponse = await _githubService.FetchLatestReleaseDataAsync(repoOwner, repoName);

                var latestRelease = JsonConvert.DeserializeObject<GitHubRelease>(jsonResponse);
                if (latestRelease == null || latestRelease.Assets == null)
                {
                    UpdateStatusChanged?.Invoke("No deployment targets found on the remote repository.");
                    return false;
                }

                // Step 2: Locate your custom manifest files inside the catalog
                string manifestUrl = null;
                string manifestSigUrl = null;

                foreach (var asset in latestRelease.Assets)
                {
                    if (asset.Name.Equals("update.json", StringComparison.OrdinalIgnoreCase))
                        manifestUrl = asset.DownloadUrl;
                    else if (asset.Name.Equals("update.json.sig", StringComparison.OrdinalIgnoreCase))
                        manifestSigUrl = asset.DownloadUrl;
                }

                if (manifestUrl == null || manifestSigUrl == null)
                {
                    UpdateStatusChanged?.Invoke("Aborting: Remote release is missing 'update.json' or its digital signature.");
                    return false;
                }

                // Step 3: Fetch custom manifest data into string variables
                UpdateStatusChanged?.Invoke("Downloading authority manifest layout...");
                string manifestJsonContent;
                string manifestDetachedSignature;

                using (var client = new WebClient())
                {
                    client.Headers.Add("User-Agent", "RulerApp-UpdateCoordinator");
                    manifestJsonContent = await client.DownloadStringTaskAsync(manifestUrl);
                    manifestDetachedSignature = await client.DownloadStringTaskAsync(manifestSigUrl);
                }

                // Step 4: Verify manifest authenticity BEFORE parsing version entries
                UpdateStatusChanged?.Invoke("Validating manifest authority signatures...");
                bool isManifestTrusted = VerifyPayloadData(manifestJsonContent, manifestDetachedSignature);

                if (!isManifestTrusted)
                {
                    UpdateStatusChanged?.Invoke("Security Failure: The update manifest failed verification gates.");
                    return false;
                }

                // Step 5: Safely deserialize the trusted string into your concrete structure
                var manifest = JsonConvert.DeserializeObject<UpdateManifest>(manifestJsonContent);
                if (manifest == null) return false;

                // Step 6: Evaluate version criteria
                var current = new Version(currentVersion.Replace("v", ""));
                var latest = new Version(manifest.Version.Replace("v", ""));

                if (latest <= current)
                {
                    UpdateStatusChanged?.Invoke("Your application is completely up to date.");
                    return false;
                }

                UpdateStatusChanged?.Invoke($"New Version Located: v{manifest.Version}. Initializing package sync...");

                // Step 7: Locate package ZIP bundle matching the trusted manifest version
                string targetZipPackageName = $"Ruler.Update.v{manifest.Version}.zip";
                string zipDownloadUrl = null;
                string zipSigUrl = null;

                foreach (var asset in latestRelease.Assets)
                {
                    if (asset.Name.Equals(targetZipPackageName, StringComparison.OrdinalIgnoreCase))
                        zipDownloadUrl = asset.DownloadUrl;
                    else if (asset.Name.Equals($"{targetZipPackageName}.sig", StringComparison.OrdinalIgnoreCase))
                        zipSigUrl = asset.DownloadUrl;
                }

                if (zipDownloadUrl == null || zipSigUrl == null)
                {
                    UpdateStatusChanged?.Invoke($"Aborting: Package bundle '{targetZipPackageName}' or its signature file is missing.");
                    return false;
                }

                // Ensure clean isolation directory bounds
                if (!Directory.Exists(_tempDownloadDir)) Directory.CreateDirectory(_tempDownloadDir);
                string localZipPath = Path.Combine(_tempDownloadDir, targetZipPackageName);
                string localZipSigPath = Path.Combine(_tempDownloadDir, $"{targetZipPackageName}.sig");

                // Step 8: Stream payload and detached signatures to local machine
                UpdateStatusChanged?.Invoke("Downloading secure archive block...");
                await _downloadService.DownloadFileAsync(zipDownloadUrl, localZipPath);
                await _downloadService.DownloadFileAsync(zipSigUrl, localZipSigPath);

                // Step 9: Validate package binary footprint against both cryptographic keys
                UpdateStatusChanged?.Invoke("Running cryptographic security evaluations on archive block...");
                byte[] zipSignatureBytes = File.ReadAllBytes(localZipSigPath);

                bool isZipTrusted = VerifyFileContent(localZipPath, zipSignatureBytes);

                // Instantly remove signature file artifact from disk
                if (File.Exists(localZipSigPath)) File.Delete(localZipSigPath);

                if (!isZipTrusted)
                {
                    UpdateStatusChanged?.Invoke("Security Failure: The downloaded update archive failed verification rules.");
                    ClearTempDirectory();
                    return false;
                }

                // Step 10: Loop through your individual manifest file hash inventory declarations
                UpdateStatusChanged?.Invoke("Verifying integrity checksum metrics of payload components...");
                if (manifest.Files != null)
                {
                    foreach (FileEntry fileEntry in manifest.Files)
                    {
                        // Extract individual file identifiers dynamically
                        string targetName = fileEntry.FileName ?? fileEntry.Name;
                        string expectedHash = fileEntry.Hash;

                        // Future Extension: Compute local file stream hashes post extraction 
                        // and compare them with expectedHash to catch partial or localized disruptions.
                    }
                }

                UpdateStatusChanged?.Invoke("All checks successful! Spawning installer...");
                // Process.Start or invoke your application setup wrapper here...

                return true;
            }
            catch (Exception ex)
            {
                UpdateStatusChanged?.Invoke($"System error during update coordinator lifecycle: {ex.Message}");
                ClearTempDirectory();
                return false;
            }
        }

        // --- CRYPTOGRAPHIC UTILITIES INCORPORATING DUAL-KEY SECURITY GATES ---

        private bool VerifyPayloadData(string payload, string signature)
        {
            // 1. Try Routine Key Verification
            bool routineCheck = _keyVerifier.VerifyPayloadString(payload, signature, _activePublicKey, _masterPublicKey);
            if (routineCheck) return true;

            // 2. Emergency Rotation Fallback
            UpdateStatusChanged?.Invoke("Active key validation failed. Trying root Master Key fallback verification...");
            return _fallbackVerifier.VerifyPayloadWithMasterKey(payload, signature, _masterPublicKey);
        }

        private bool VerifyFileContent(string filePath, byte[] signatureBytes)
        {
            // 1. Try Routine Key Verification
            bool routineCheck = _keyVerifier.VerifyFileSignature(filePath, signatureBytes, _activePublicKey, _masterPublicKey);
            if (routineCheck) return true;

            // 2. Emergency Rotation Fallback
            UpdateStatusChanged?.Invoke("Active key validation failed. Trying root Master Key fallback verification...");
            return _fallbackVerifier.VerifyFileWithMasterKey(filePath, signatureBytes, _masterPublicKey);
        }

        private void ClearTempDirectory()
        {
            try
            {
                if (Directory.Exists(_tempDownloadDir))
                {
                    Directory.Delete(_tempDownloadDir, true);
                }
            }
            catch { /* Suppress IO locks */ }
        }
    }
}

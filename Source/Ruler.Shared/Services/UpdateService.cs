using Newtonsoft.Json;

using Ruler.Shared.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Services
{
    public class UpdateService
    {
        private readonly HttpClient _httpClient;
        private readonly SecurityService _securityService;
        private readonly DownloadService _downloadService;
        private readonly ArchiveService _archiveService;

        // Configuration - Update these for your specific repo
        private const string RepoUser = "YOUR_USERNAME";
        private const string RepoName = "YOUR_REPO_NAME";
        private const string GitHubApiUrl = $"https://api.github.com/repos/{RepoUser}/{RepoName}/releases/latest";

        public UpdateService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Ruler-App-Updater");

            // Initialize the sub-services
            _securityService = new SecurityService();
            _downloadService = new DownloadService();
            _archiveService = new ArchiveService();
        }

        /// <summary>
        /// The Master Method: Checks, Verifies, Downloads, and Prepares the Update.
        /// </summary>
        public async Task<bool> RunUpdateWorkflowAsync(string currentVersion, IProgress<double> progress)
        {
            try
            {
                // 1. Check GitHub for the latest release
                var release = await GetLatestReleaseAsync();
                if (release == null || release.TagName == currentVersion) return false;

                // 2. Download and Verify Manifest (The small security files)
                var manifest = await DownloadAndVerifyManifestAsync(release);
                if (manifest == null) throw new Exception("Security verification failed: Invalid Manifest.");

                // 3. Download the main ZIP package
                var zipAsset = release.Assets.FirstOrDefault(a => a.Name.EndsWith(".zip"));
                string tempZipPath = Path.Combine(Path.GetTempPath(), "RulerUpdate", zipAsset.Name);

                await _downloadService.DownloadFileAsync(zipAsset.DownloadUrl, tempZipPath, progress);

                // 4. Verify ZIP Integrity (SHA256)
                string actualHash = _securityService.CalculateFileHash(tempZipPath);
                if (actualHash != manifest.PackageHash) throw new Exception("ZIP hash mismatch!");

                // 5. Extract files
                string extractionPath = _archiveService.ExtractUpdate(tempZipPath);

                // 6. Final Binary Check (Authenticode)
                string newExePath = Path.Combine(extractionPath, "MainApp.exe");
                if (!_securityService.IsFileAuthentic(newExePath)) throw new Exception("Binary signature invalid!");

                // 7. Hand off to Updater.exe
                LaunchUpdaterProcess(extractionPath);
                return true;
            }
            catch (Exception ex)
            {
                // Log ex.Message here
                return false;
            }
        }

        private async Task<GitHubRelease> GetLatestReleaseAsync()
        {
            var response = await _httpClient.GetStringAsync(GitHubApiUrl);
            return JsonConvert.DeserializeObject<GitHubRelease>(response);
        }

        private async Task<UpdateManifest> DownloadAndVerifyManifestAsync(GitHubRelease release)
        {
            var mAsset = release.Assets.FirstOrDefault(a => a.Name == "manifest.json");
            var sAsset = release.Assets.FirstOrDefault(a => a.Name == "manifest.json.sig");

            if (mAsset == null || sAsset == null) return null;

            string json = await _httpClient.GetStringAsync(mAsset.DownloadUrl);
            string signature = await _httpClient.GetStringAsync(sAsset.DownloadUrl);

            if (_securityService.VerifyManifestSignature(json, signature))
            {
                return JsonConvert.DeserializeObject<UpdateManifest>(json);
            }
            return null;
        }

        private void LaunchUpdaterProcess(string extractedFolder)
        {
            string updaterPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Updater.exe");
            int currentPid = System.Diagnostics.Process.GetCurrentProcess().Id;
            string targetDir = AppDomain.CurrentDomain.BaseDirectory;

            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = updaterPath,
                Arguments = $"\"{extractedFolder}\" \"{targetDir}\" {currentPid}",
                UseShellExecute = true,
                Verb = "runas" // Request Admin for file replacement
            };

            System.Diagnostics.Process.Start(startInfo);
            System.Environment.Exit(0);
        }
    }
}

using Newtonsoft.Json;

using Ruler.Shared.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Services
{
    public class UpdateService
    {
        private readonly HttpClient _httpClient;
        private const string RepoOwner = "IsaacMorris"; // Your GitHub Username
        private const string RepoName = "Ruler";       // Your Repo Name

        public UpdateService()
        {
            _httpClient = new HttpClient();
            // GitHub API requires a User-Agent header
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Ruler-Update-Service");
        }

        /// <summary>
        /// The main entry point called during App Launch.
        /// </summary>
        public async Task<bool> CheckAndPerformUpdateAsync(string currentVersion)
        {
            try
            {
                // 1. Get the latest release info from GitHub
                var release = await GetLatestGitHubReleaseAsync();
                if (release == null || new Version(release.TagName.TrimStart('v')) <= new Version(currentVersion))
                    return false;

                // 2. Locate the Manifest and Zip in the release assets
                string manifestUrl = null;
                string zipUrl = null;

                foreach (var asset in release.Assets)
                {
                    if (asset.Name == "update.json") manifestUrl = asset.BrowserDownloadUrl;
                    if (asset.Name.EndsWith(".zip")) zipUrl = asset.BrowserDownloadUrl;
                }

                if (manifestUrl == null || zipUrl == null) return false;

                // 3. Download and Verify the Manifest
                var manifestJson = await _httpClient.GetStringAsync(manifestUrl);
                var manifest = JsonConvert.DeserializeObject<UpdateManifest>(manifestJson);

                // 4. Download Zip to Temp folder
                string tempZip = Path.Combine(Path.GetTempPath(), "RulerUpdate.zip");
                var zipData = await _httpClient.GetByteArrayAsync(zipUrl);
                File.WriteAllBytes(tempZip, zipData);

                // 5. Verify ZIP Trust (RSA check against your Developer Public Key)
                if (!SecurityService.VerifyExternalTrust(tempZip, manifest.ZipSignature))
                {
                    throw new Exception("Update Package signature is untrusted!");
                }

                // 6. Extract and Verify Individual Files
                string extractPath = Path.Combine(Path.GetTempPath(), "Ruler_Staging");
                if (Directory.Exists(extractPath)) Directory.Delete(extractPath, true);
                ZipFile.ExtractToDirectory(tempZip, extractPath);

                foreach (var fileMeta in manifest.Files)
                {
                    string filePath = Path.Combine(extractPath, fileMeta.FileName);
                    if (!File.Exists(filePath)) throw new Exception($"Missing file: {fileMeta.FileName}");

                    // Check SHA256 Hash
                    if (SecurityService.GenerateHash(filePath) != fileMeta.Hash)
                        throw new Exception($"Integrity check failed for {fileMeta.FileName}");
                }

                // 7. Success! Launch the Updater/Swapper and exit the main app
                LaunchUpdater(extractPath);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update Error: {ex.Message}");
                return false;
            }
        }

        private async Task<GitHubRelease> GetLatestGitHubReleaseAsync()
        {
            string url = $"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest";
            var response = await _httpClient.GetStringAsync(url);
            return JsonConvert.DeserializeObject<GitHubRelease>(response);
        }

        private void LaunchUpdater(string stagingFolder)
        {
            // This would launch your Ruler.Updater.exe passing the staging path
            // Then Application.Current.Shutdown();
        }
    }
}

using Newtonsoft.Json;

using Ruler.Shared.Models;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
                string manifestSignatureUrl = null;
                foreach (var asset in release.Assets)
                {
                    if (asset.Name == "manifest.json") manifestUrl = asset.BrowserDownloadUrl;
                    if (asset.Name == "manifest.json.sig") manifestSignatureUrl = asset.BrowserDownloadUrl;
                    if (asset.Name.EndsWith(".zip")) zipUrl = asset.BrowserDownloadUrl;
                }

                if (manifestUrl == null || zipUrl == null) return false;

                // 3. Download and Verify the Manifest
                var manifestJson = await _httpClient.GetStringAsync(manifestUrl);
                UpdateManifest manifest = JsonConvert.DeserializeObject<UpdateManifest>(manifestJson);
               ;
                string manifestSignature = await _httpClient.GetStringAsync(manifestSignatureUrl);

                if (!SecurityService.VerifyTrust(manifestJson, manifestSignature))
                {
                    throw new Exception("The Update Manifest itself is untrusted or has been tampered with!");
                }

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

        private void LaunchUpdater(string stagingPath)
        {
            // 1. Locate the physical Updater.exe relative to where Ruler.exe is running
            string updaterPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Updater.exe");

            if (!File.Exists(updaterPath))
            {
                // Fallback: if the updater is missing, we can't update.
                // You might want to log this or alert the user.
                return;
            }

            // 2. Prepare the command line arguments:
            // Arg 0: The folder where the new files are waiting (Staging)
            // Arg 1: The folder where the app lives (Destination)
            // Arg 2: This app's Process ID (so the updater can wait for us to exit)
            string destinationPath = AppDomain.CurrentDomain.BaseDirectory;
            int currentPid = Process.GetCurrentProcess().Id;

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = updaterPath,
                Arguments = $"\"{stagingPath}\" \"{destinationPath}\" {currentPid}",
                UseShellExecute = false,  // Directly start the process
                CreateNoWindow = true,   // Don't show a console window
                WindowStyle = ProcessWindowStyle.Hidden
            };

            try
            {
                // 3. Launch the updater
                Process.Start(startInfo);

                // 4. Kill the current application immediately
                // This releases the locks on Ruler.exe and Ruler.Shared.dll
                Application.Exit();
            }
            catch (Exception ex)
            {
                // Log the failure to launch
                Debug.WriteLine($"Critical Error: Could not start Updater.exe. {ex.Message}");
            }
        }
    }
}

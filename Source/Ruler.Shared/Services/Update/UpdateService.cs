using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Ruler.Contracts.Models.DTO;

using Ruler.Contracts.Services.Security;
using Ruler.Contracts.Services.Updates;

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Ruler.Wpf.Services
{
    public class UpdateService : IUpdateService
    {
        private readonly IReleaseValidationService _releaseValidator;
        private bool _isUpdateAvailable;
        private UpdateManifest _verifiedManifest;

        // Paths for tracking local update artifacts
        private readonly string _localManifestPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "latest.json");
        private readonly string _localSignaturePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "latest.json.sig");
        private readonly string _zipPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "update.zip");
        private readonly string _tempExtractPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UpdateTemp");

        public bool UpdateAvailable => _isUpdateAvailable;

        public UpdateService(IReleaseValidationService releaseValidator)
        {
            _releaseValidator = releaseValidator;
        }

        // --- CHECKPOINT 1: FETCH GITHUB ASSETS & VALIDATE MANIFEST SIGNATURE ---
        public async Task CheckForUpdatesAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "RulerScreenApp-Updater");
                string githubApiUrl = "https://api.github.com/repos/isaacmorrisdev/ruler/releases/latest";

                try
                {
                    string apiJsonResponse = await client.GetStringAsync(githubApiUrl).ConfigureAwait(false);

                    var githubSettings = new JsonSerializerSettings
                    {
                        ContractResolver = new DefaultContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() }
                    };
                    var githubRelease = JsonConvert.DeserializeObject<GitHubRelease>(apiJsonResponse, githubSettings);

                    if (githubRelease == null || githubRelease.Assets == null) return;

                    var manifestAsset = githubRelease.Assets.FirstOrDefault(a => a.Name.Equals("latest.json", StringComparison.OrdinalIgnoreCase));
                    var signatureAsset = githubRelease.Assets.FirstOrDefault(a => a.Name.Equals("latest.json.sig", StringComparison.OrdinalIgnoreCase));

                    if (manifestAsset == null || signatureAsset == null) return;

                    string manifestJson = await client.GetStringAsync(manifestAsset.DownloadUrl).ConfigureAwait(false);
                    string base64Signature = await client.GetStringAsync(signatureAsset.DownloadUrl).ConfigureAwait(false);

                    // .NET Framework 4.8 compliant Asynchronous file writing via streams
                    using (FileStream fs = new FileStream(_localManifestPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
                    using (StreamWriter writer = new StreamWriter(fs,System.Text.Encoding.UTF8))
                    {
                        await writer.WriteAsync(manifestJson).ConfigureAwait(false);
                    }

                    using (FileStream fs = new FileStream(_localSignaturePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
                    using (StreamWriter writer = new StreamWriter(fs, System.Text.Encoding.UTF8))
                    {
                        await writer.WriteAsync(base64Signature).ConfigureAwait(false);
                    }

                    // Cryptographic validation of the manifest string payload
                    if (!_releaseValidator.VerifyStringSignature(manifestJson, base64Signature.Trim()))
                    {
                        throw new System.Security.Cryptography.CryptographicException("CRITICAL SECURITY FAILURE: Manifest file signature mismatch!");
                    }

                    var manifestSettings = new JsonSerializerSettings
                    {
                        ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }
                    };
                    _verifiedManifest = JsonConvert.DeserializeObject<UpdateManifest>(manifestJson, manifestSettings);
                    if (_verifiedManifest == null) return;

                    Version currentVersion = Assembly.GetEntryAssembly().GetName().Version;
                    Version latestVersion = new Version(_verifiedManifest.Version.TrimStart('v'));

                    if (latestVersion > currentVersion)
                    {
                        _isUpdateAvailable = true;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Update check pipeline terminated: " + ex.Message);
                    PurgeEverything();
                }
            }
        }

        // --- CHECKPOINT 2: DOWNLOAD THE WRAPPER ZIP PACKAGE & VERIFY CONTAINER SIGNATURE ---
        public async Task<bool> DownloadUpdateAsync()
        {
            if (_verifiedManifest == null) return false;

            try
            {
                string downloadUrl = "https://github.com/isaacmorrisdev/ruler/releases/latest/download/" + _verifiedManifest.ZipPackageName;

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "RulerScreenApp-Updater");

                    using (HttpResponseMessage response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
                    {
                        response.EnsureSuccessStatusCode();

                        using (Stream networkStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                        using (Stream diskStream = new FileStream(_zipPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
                        {
                            await networkStream.CopyToAsync(diskStream).ConfigureAwait(false);
                        }
                    }
                }

                // Verify the zipped archive envelope itself before unzipping any contents
                if (!_releaseValidator.VerifyFileSignature(_zipPath, _verifiedManifest.ZipSignature))
                {
                    throw new System.Security.Cryptography.CryptographicException("SECURITY BREACH: Downloaded update.zip failed envelope validation!");
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Download pipeline failed: " + ex.Message);
                PurgeEverything();
                return false;
            }
        }

        // --- CHECKPOINT 3: EXTRACT, CHIP-LEVEL VERIFY BINARIES, AND SELF-HEAL ---
        public void InstallUpdate()
        {
            if (!File.Exists(_zipPath) || _verifiedManifest == null) return;

            try
            {
                if (Directory.Exists(_tempExtractPath)) Directory.Delete(_tempExtractPath, true);
                Directory.CreateDirectory(_tempExtractPath);

                // Note: ZipFile requires a reference to System.IO.Compression.FileSystem assembly in .NET 4.8
                ZipFile.ExtractToDirectory(_zipPath, _tempExtractPath);

                string newMainExePath = Path.Combine(_tempExtractPath, "Ruler.exe");
                string newWorkerExePath = Path.Combine(_tempExtractPath, "Ruler.Worker.exe");

                // Prevent Directory Traversal attempts within the zip structure
                if (!_releaseValidator.IsPathSafe(_tempExtractPath, newMainExePath) ||
                    !_releaseValidator.IsPathSafe(_tempExtractPath, newWorkerExePath))
                {
                    throw new System.Security.SecurityException("Malicious file layout paths detected inside extraction footprint!");
                }

                // Dynamic all-or-nothing signature matching loop over your FileEntry array
                foreach (var fileEntry in _verifiedManifest.Files)
                {
                    string absoluteTargetFilePath = Path.Combine(_tempExtractPath, fileEntry.Path);

                    if (!File.Exists(absoluteTargetFilePath))
                    {
                        throw new FileNotFoundException($"Missing expected update binary component: {fileEntry.Path}");
                    }

                    if (!_releaseValidator.VerifyFileSignature(absoluteTargetFilePath, fileEntry.Signature))
                    {
                        throw new System.Security.Cryptography.CryptographicException($"SECURITY BREACH: {fileEntry.Path} failed individual verification!");
                    }
                }

                // Smart Check: Verify if Ruler.Worker needs to be executed out of temp or local directory
                string currentWorkerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Ruler.Worker.exe");
                bool shouldReplaceWorker = true;

                if (File.Exists(currentWorkerPath) && File.Exists(newWorkerExePath))
                {
                    Version currentWorkerVersion = new Version(FileVersionInfo.GetVersionInfo(currentWorkerPath).FileVersion);
                    Version extractedWorkerVersion = new Version(FileVersionInfo.GetVersionInfo(newWorkerExePath).FileVersion);

                    if (currentWorkerVersion >= extractedWorkerVersion)
                    {
                        shouldReplaceWorker = false;
                        File.Delete(newWorkerExePath); // Clean it up right away out of the staging sandbox
                    }
                }

                // Verify the backup path safety just in case of file loss/AV deletions
                if (!shouldReplaceWorker && !File.Exists(currentWorkerPath))
                {
                    throw new FileNotFoundException("Self-Healing Routine Blocked: Production execution worker missing from installation.");
                }

                string executableWorkerToLaunch = shouldReplaceWorker ? newWorkerExePath : currentWorkerPath;
                LaunchWorkerUpdater(executableWorkerToLaunch, _tempExtractPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Installation chain terminated due to failure: " + ex.Message);
                PurgeEverything();
            }
        }

        // --- OPTION A PERIMETER: AUTOMATED TOTAL SLATE PURGE ---
        private void PurgeEverything()
        {
            _isUpdateAvailable = false;
            _verifiedManifest = null;

            TrySafeDeleteFile(_localManifestPath);
            TrySafeDeleteFile(_localSignaturePath);
            TrySafeDeleteFile(_zipPath);
            TrySafeDeleteDirectory(_tempExtractPath);

            Debug.WriteLine("Option A Security Purge Executed: System state completely reset.");
        }

        private void TrySafeDeleteFile(string path)
        {
            try { if (File.Exists(path)) File.Delete(path); }
            catch (Exception ex) { Debug.WriteLine($"Purge failed to delete file: {path}. Error: {ex.Message}"); }
        }

        private void TrySafeDeleteDirectory(string path)
        {
            try { if (Directory.Exists(path)) Directory.Delete(path, true); }
            catch (Exception ex) { Debug.WriteLine($"Purge failed to remove directory: {path}. Error: {ex.Message}"); }
        }

        private void LaunchWorkerUpdater(string workerPath, string stagingPath)
        {
            Debug.WriteLine($"Handoff complete. Launching worker: {workerPath} with payload context: {stagingPath}");
        }
    }
}

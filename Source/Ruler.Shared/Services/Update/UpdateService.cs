using Newtonsoft.Json;

using Ruler.Contracts.Services.Security;
using Ruler.Contracts.Services.Updates;
using Ruler.Shared;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Threading.Tasks;
namespace Ruler.Wpf.Services
{
    public class UpdateService : IUpdateService
    {
        private readonly IReleaseValidationService _releaseValidator;
        private bool _isUpdateAvailable;
        private VerifiedReleaseManifest _latestRelease;

        public bool UpdateAvailable => _isUpdateAvailable;

        public UpdateService(IReleaseValidationService releaseValidator)
        {
            _releaseValidator = releaseValidator;
        }
        // --- CHECKPOINT 1: VALIDATE THE MANIFEST TEXT BEFORE TRUSTING IT ---
        public async Task CheckForUpdatesAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "RulerScreenApp-Updater");

                string manifestUrl = "https://yourdomain.com/updates/latest.json";
                string signatureUrl = "https://yourdomain.com/updates/latest.json.sig";

                try
                {
                    string manifestJson = await client.GetStringAsync(manifestUrl).ConfigureAwait(false);
                    string base64Signature = await client.GetStringAsync(signatureUrl).ConfigureAwait(false);

                    // Validate text payload via detached signature string
                    if (!_releaseValidator.VerifyStringSignature(manifestJson, base64Signature.Trim()))
                    {
                        Debug.WriteLine("CRITICAL SECURITY FAILURE: Manifest metadata file signature mismatch!");
                        return; // Halt immediately
                    }

                    _latestRelease = JsonConvert.DeserializeObject<VerifiedReleaseManifest>(manifestJson);
                    if (_latestRelease == null) return;

                    Version currentVersion = Assembly.GetEntryAssembly().GetName().Version;
                    Version latestVersion = new Version(_latestRelease.TagName.TrimStart('v'));

                    if (latestVersion > currentVersion)
                    {
                        _isUpdateAvailable = true;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Failed update pipeline network validation: " + ex.Message);
                }
            }
        }

        // --- CHECKPOINT 2: VALIDATE THE ZIP CONTAINER BEFORE EXTRACTION ---
        public async Task<bool> DownloadUpdateAsync()
        {
            if (_latestRelease == null || _latestRelease.Assets == null || !_latestRelease.Assets.Any())
                return false;

            try
            {
                string localZipPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "update.zip");
                string downloadUrl = _latestRelease.Assets.First().DownloadUrl;

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "RulerScreenApp-Updater");
                    using (Stream networkStream = await client.GetStreamAsync(downloadUrl).ConfigureAwait(false))
                    using (Stream diskStream = File.Open(localZipPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await networkStream.CopyToAsync(diskStream).ConfigureAwait(false);
                    }
                }

                // Verify file payload using our memory-safe streaming verification algorithm
                if (!_releaseValidator.VerifyFileSignature(localZipPath, _latestRelease.ZipSignature))
                {
                    Debug.WriteLine("SECURITY BREACH: Downloaded update.zip failed signature validation.");
                    if (File.Exists(localZipPath)) File.Delete(localZipPath);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Download operation failed: " + ex.Message);
                return false;
            }
        }

        // --- CHECKPOINT 3: VALIDATE INDIVIDUAL EXECUTABLES BEFORE HANDOVER ---
        public void InstallUpdate()
        {
            string zipPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "update.zip");
            string tempExtractPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UpdateTemp");

            if (!File.Exists(zipPath)) return;

            try
            {
                if (Directory.Exists(tempExtractPath)) Directory.Delete(tempExtractPath, true);
                Directory.CreateDirectory(tempExtractPath);

                ZipFile.ExtractToDirectory(zipPath, tempExtractPath);

                string newMainExePath = Path.Combine(tempExtractPath, "Ruler.exe");
                string newWorkerExePath = Path.Combine(tempExtractPath, "Ruler.Worker.exe");

                // Enforce strict path checks to protect against directory traversal
                if (!_releaseValidator.IsPathSafe(tempExtractPath, newMainExePath) ||
                    !_releaseValidator.IsPathSafe(tempExtractPath, newWorkerExePath))
                {
                    throw new System.Security.SecurityException("Malicious extraction path detected!");
                }

                // Validate both individual binary execution payloads
                if (!_releaseValidator.VerifyFileSignature(newMainExePath, _latestRelease.MainExeSignature) ||
                    !_releaseValidator.VerifyFileSignature(newWorkerExePath, _latestRelease.SecondaryExeSignature))
                {
                    Directory.Delete(tempExtractPath, true);
                    throw new CryptographicException("SECURITY BREACH: Extracted application binaries failed validation.");
                }

                // Hand off system control safely to the unzipped worker console program
                LaunchWorkerUpdater(newWorkerExePath, tempExtractPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Installation chain terminated: " + ex.Message);
            }
        }

        private void LaunchWorkerUpdater(string tempWorkerPath, string tempExtractPath)
        {
            string currentAppDir = AppDomain.CurrentDomain.BaseDirectory;
            int currentProcessId = Process.GetCurrentProcess().Id;

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = tempWorkerPath,
                Arguments = $"--pid {currentProcessId} --target-dir \"{currentAppDir}\" --temp-dir \"{tempExtractPath}\"",
                UseShellExecute = true,
                CreateNoWindow = false // Set to true to hide console output window completely from user view
            };

            Process.Start(psi);
            Environment.Exit(0); // Exit immediately to drop unmanaged Win32 application file locks
        }
    }
}

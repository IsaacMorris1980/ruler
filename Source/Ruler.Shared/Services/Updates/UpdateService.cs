using Newtonsoft.Json;

using Ruler.Contracts.Models;
using Ruler.Contracts.Services.Updates;
using Ruler.Contracts.Services.Verification;
using Ruler.Shared.Models;

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
using System.Text;
using System.Threading.Tasks;
using Ruler.Contracts.Services.Verification;

namespace Ruler.Shared.Services.Updates
{
    public class UpdateService
    {
        private readonly IReleaseValidationService _releaseValidator;
        private bool _isUpdateAvailable;
        private UpdateManifest _latestRelease;

        public bool UpdateAvailable => _isUpdateAvailable;

        public UpdateService(IReleaseValidationService releaseValidator)
        {
            _releaseValidator = releaseValidator;
        }

        // --- CHECKPOINT 1: VALIDATE DETACHED MANIFEST SIGNATURE ---
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

                    // Checkpoint 1 Gate
                    if (!_releaseValidator.VerifyStringSignature(manifestJson, base64Signature.Trim()))
                    {
                        Debug.WriteLine("CRITICAL SECURITY FAILURE: Manifest file signature verification failed!");
                        return;
                    }

                    // Manifest is verified genuine. Safe to deserialize!
                    _latestRelease = JsonConvert.DeserializeObject<UpdateManifest>(manifestJson);
                    if (_latestRelease == null) return;

                    Version currentVersion = Assembly.GetEntryAssembly().GetName().Version;
                    Version latestVersion = new Version(_latestRelease.Version.TrimStart('v'));

                    if (latestVersion > currentVersion)
                    {
                        _isUpdateAvailable = true;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Failed update pipeline check: " + ex.Message);
                }
            }
        }

        // --- CHECKPOINT 2: VALIDATE ZIP CONTAINER ---
        public async Task<bool> DownloadUpdateAsync()
        {
            if (_latestRelease == null) return false;

            try
            {
                string localZipPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "update.zip");

                // Assuming your download URL is mapped or standardized by version name
                string downloadUrl = $"https://yourdomain.com/updates/releases/{_latestRelease.Version}.zip";

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "RulerScreenApp-Updater");
                    using (Stream networkStream = await client.GetStreamAsync(downloadUrl).ConfigureAwait(false))
                    using (Stream diskStream = File.Open(localZipPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await networkStream.CopyToAsync(diskStream).ConfigureAwait(false);
                    }
                }

                // Checkpoint 2 Gate: Verify zip package signature via the cascading key service
                if (!_releaseValidator.VerifyFileSignature(localZipPath, _latestRelease.PackageHash))
                {
                    Debug.WriteLine("SECURITY BREACH: Downloaded package zip failed signature validation.");
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

        // --- CHECKPOINT 3: DYNAMICALLY VALIDATE ALL EXTRACTED EXECUTABLES ---
        public void InstallUpdate()
        {
            string currentAppDir = AppDomain.CurrentDomain.BaseDirectory;
            string zipPath = Path.Combine(currentAppDir, "update.zip");
            string tempExtractPath = Path.Combine(currentAppDir, "UpdateTemp");

            if (!File.Exists(zipPath) || _latestRelease?.Files == null) return;

            try
            {
                if (Directory.Exists(tempExtractPath)) Directory.Delete(tempExtractPath, true);
                Directory.CreateDirectory(tempExtractPath);

                // 1. Extract package files into the temp folder
                ZipFile.ExtractToDirectory(zipPath, tempExtractPath);

                // 2. Validate all files dynamically against their signatures
                foreach (FileEntry fileEntry in _latestRelease.Files)
                {
                    string absoluteExtractedPath = Path.Combine(tempExtractPath, fileEntry.FileName);

                    if (!_releaseValidator.IsPathSafe(tempExtractPath, absoluteExtractedPath))
                        throw new System.Security.SecurityException($"Malicious path layout intercepted: {fileEntry.FileName}");

                    if (!File.Exists(absoluteExtractedPath))
                        throw new FileNotFoundException($"Missing deployment target asset: {fileEntry.FileName}");

                    if (!_releaseValidator.VerifyFileSignature(absoluteExtractedPath, fileEntry.Hash))
                    {
                        Directory.Delete(tempExtractPath, true);
                        throw new CryptographicException($"Signature verification failed for: {fileEntry.FileName}");
                    }
                }

                // 3. Find the extracted worker binary
                FileEntry workerAsset = _latestRelease.Files.FirstOrDefault(f => f.FileName.Equals("Ruler.Worker.exe", StringComparison.OrdinalIgnoreCase));
                if (workerAsset == null) throw new FileNotFoundException("Ruler.Worker.exe was missing from the release manifest.");

                string extractedWorkerPath = Path.Combine(tempExtractPath, workerAsset.FileName);
                string permanentWorkerPath = Path.Combine(currentAppDir, "Ruler.Worker.exe");

                // --- STEP A: UPDATE RULER.WORKER.EXE FIRST ---
                // Overwrite the old idle worker in the installation directory with the new one
                File.Copy(extractedWorkerPath, permanentWorkerPath, overwrite: true);

                // --- STEP B: EXECUTE THE NEW WORKER FILE TO UPDATE RULER.EXE ---
                LaunchUpdatedWorker(permanentWorkerPath, tempExtractPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Installation chain terminated: " + ex.Message);
            }
        }

        private void LaunchUpdatedWorker(string permanentWorkerPath, string tempExtractPath)
        {
            string currentAppDir = AppDomain.CurrentDomain.BaseDirectory;
            int currentProcessId = Process.GetCurrentProcess().Id;

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = permanentWorkerPath, // Executes the newly updated permanent worker file
                Arguments = $"--pid {currentProcessId} --target-dir \"{currentAppDir}\" --temp-dir \"{tempExtractPath}\"",
                UseShellExecute = false,        // Direct process execution bypasses the command shell
                CreateNoWindow = false          // Keeps the console window open for visibility
            };

            Process.Start(psi);
            Environment.Exit(0); // Immediately close Ruler.exe to drop OS file locks
        }
    }
}

using Newtonsoft.Json;

using Ruler.Shared.Models;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Ruler.Shared.Services
{
    public static class UpdateService
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        /// <summary>
        /// Checks the remote manifest to see if a newer version is available compared to the current app version.
        /// </summary>
        public static async Task<(bool UpdateAvailable, string LatestVersion)> CheckForUpdateAsync(string manifestUrl, string currentVersion)
        {
            try
            {
                string jsonResponse = await HttpClient.GetStringAsync(manifestUrl);
                var manifest = JsonConvert.DeserializeObject<UpdateManifest>(jsonResponse);

                if (manifest == null || string.IsNullOrEmpty(manifest.ReleaseVersion))
                {
                    return (false, null);
                }

                Version currentVer = new Version(currentVersion);
                Version remoteVer = new Version(manifest.ReleaseVersion);

                if (remoteVer > currentVer)
                {
                    return (true, manifest.ReleaseVersion);
                }
            }
            catch
            {
                // Suppress network errors during background checks
            }

            return (false, null);
        }

        /// <summary>
        /// Downloads, extracts, cryptographically verifies the update package[cite: 1, 2], 
        /// and launches Ruler.Updater to perform the file replacement.
        /// </summary>
        public static async Task DownloadAndApplyUpdateAsync(string manifestUrl, string zipUrl, string targetAppDirectory)
        {
            string stagingDirectory = Path.Combine(Path.GetTempPath(), "RulerUpdateStaging_" + Guid.NewGuid());
            string tempZipPath = Path.Combine(Path.GetTempPath(), $"RulerUpdate_{Guid.NewGuid()}.zip");
            bool validationSuccessful = false;

            try
            {
                // 1. Download and deserialize the update manifest using Newtonsoft.Json[cite: 2]
                string jsonResponse = await HttpClient.GetStringAsync(manifestUrl);
                var manifest = JsonConvert.DeserializeObject<UpdateManifest>(jsonResponse);

                if (manifest == null || string.IsNullOrEmpty(manifest.ZipSignature) || manifest.Files == null)
                {
                    throw new InvalidOperationException("Invalid update manifest structure.");
                }

                // 2. Download the ZIP archive bytes asynchronously
                byte[] zipBytes = await HttpClient.GetByteArrayAsync(zipUrl);
                await Task.Run(() => File.WriteAllBytes(tempZipPath, zipBytes));

                // 3. Verify the overall ZIP package signature using the hardcoded Active public key
                bool isZipValid = SecurityService.VerifyFileTrust(tempZipPath, manifest.ZipSignature);
                if (!isZipValid)
                {
                    throw new SecurityException("CRITICAL: ZIP package signature verification failed! Update rejected.");
                }

                // 4. Extract the ZIP package to a temporary staging folder
                Directory.CreateDirectory(stagingDirectory);
                ZipFile.ExtractToDirectory(tempZipPath, stagingDirectory);

                // 5. Post-extraction verification: Validate every extracted file against its manifest SHA-256 hash[cite: 1, 2]
                using (var sha256 = SHA256.Create())
                {
                    foreach (var fileSig in manifest.Files)
                    {
                        string extractedFilePath = Path.Combine(stagingDirectory, fileSig.FileName);

                        if (!File.Exists(extractedFilePath))
                        {
                            throw new FileNotFoundException($"Update integrity error: Required file '{fileSig.FileName}' was missing from the archive.");
                        }

                        using (var fileStream = File.OpenRead(extractedFilePath))
                        {
                            byte[] hashBytes = sha256.ComputeHash(fileStream);
                            string computedHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

                            if (!string.Equals(computedHash, fileSig.Hash, StringComparison.OrdinalIgnoreCase))
                            {
                                throw new SecurityException($"CRITICAL: Hash mismatch for file '{fileSig.FileName}'. The file may have been corrupted or tampered with.");
                            }
                        }
                    }
                }

                // Validation passed completely. Hand over staging folder to the updater process.
                validationSuccessful = true;

                // 6. Launch Ruler.Updater and exit the application
                LaunchUpdaterAndExit(stagingDirectory, targetAppDirectory);
            }
            catch
            {
                CleanupResources(stagingDirectory, tempZipPath);
                throw;
            }
            finally
            {
                try
                {
                    if (File.Exists(tempZipPath)) File.Delete(tempZipPath);
                }
                catch { }

                if (!validationSuccessful)
                {
                    CleanupResources(stagingDirectory, null);
                }
            }
        }

        private static void LaunchUpdaterAndExit(string stagingDirectory, string targetAppDirectory)
        {
            string updaterExePath = Path.Combine(targetAppDirectory, "Ruler.Updater.exe");
            string rulerExePath = Path.Combine(targetAppDirectory, "Ruler.exe");

            if (!File.Exists(updaterExePath))
            {
                throw new FileNotFoundException("Updater executable ('Ruler.Updater.exe') not found in the application directory.");
            }

            var psi = new ProcessStartInfo
            {
                FileName = updaterExePath,
                Arguments = $"\"{rulerExePath}\" \"{stagingDirectory}\" \"{targetAppDirectory}\"",
                UseShellExecute = true
            };

            Process.Start(psi);
            Environment.Exit(0);
        }

        private static void CleanupResources(string stagingDirectory, string tempZipPath)
        {
            try
            {
                if (!string.IsNullOrEmpty(stagingDirectory) && Directory.Exists(stagingDirectory))
                {
                    Directory.Delete(stagingDirectory, recursive: true);
                }
            }
            catch
            {
                // Suppress cleanup exceptions
            }

            try
            {
                if (!string.IsNullOrEmpty(tempZipPath) && File.Exists(tempZipPath))
                {
                    File.Delete(tempZipPath);
                }
            }
            catch
            {
                // Suppress cleanup exceptions
            }
        }
    }
}

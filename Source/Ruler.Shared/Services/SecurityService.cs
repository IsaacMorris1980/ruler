using Newtonsoft.Json;

using Ruler.Shared.Models;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Services
{
    public static class SecurityService
    {
        // Embedded XML public keys replacing certificate subject lookups[cite: 6]
        private const string ActivePublicKeyXml = "<RSAKeyValue><Modulus>...</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
        private const string MasterPublicKeyXml = "<RSAKeyValue><Modulus>...</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        /// <summary>
        /// Verifies a raw byte array against a base64-encoded RSA signature using embedded XML public keys.
        /// </summary>
        public static bool VerifyData(byte[] dataBytes, string base64Signature)
        {
            if (string.IsNullOrEmpty(base64Signature)) return false;

            try
            {
                byte[] signatureBytes = Convert.FromBase64String(base64Signature);
                using (var rsa = RSA.Create())
                {
                    // 1. Try verifying with the Active public key first
                    try
                    {
                        rsa.FromXmlString(ActivePublicKeyXml);
                        if (rsa.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
                        {
                            return true;
                        }
                    }
                    catch
                    {
                        // Active key parsing or verification failed, proceed to fallback
                    }

                    // 2. Fall back to the Master public key
                    try
                    {
                        rsa.FromXmlString(MasterPublicKeyXml);
                        return rsa.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Computes the SHA-256 hash of a file and matches it against the expected hash string[cite: 6].
        /// </summary>
        public static bool VerifyFileHash(string filePath, string expectedHash)
        {
            if (!File.Exists(filePath)) return false;

            try
            {
                using (var sha256 = SHA256.Create())
                using (var stream = File.OpenRead(filePath))
                {
                    byte[] hashBytes = sha256.ComputeHash(stream);
                    string actualHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                    return string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase);
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Verifies an individual file's hash and its cryptographic signature[cite: 1, 6].
        /// </summary>
        public static bool VerifyFileSignature(string filePath, FileSignature fileSig)
        {
            if (fileSig == null || !File.Exists(filePath)) return false;

            // 1. Verify file hash matches manifest record
            if (!VerifyFileHash(filePath, fileSig.Hash))
            {
                return false;
            }

            // 2. Verify file signature bytes[cite: 1]
            byte[] fileBytes = File.ReadAllBytes(filePath);
            return VerifyData(fileBytes, fileSig.Signature);
        }

        /// <summary>
        /// Verifies the detached manifest signature, package-level zip signature, and all individual inner file signatures.
        /// If valid, deploys the new updater, launches it to update the running WPF application, and exits[cite: 6].
        /// </summary>
        public static bool VerifyAndApplyUpdatePackage(string packageDirectory, string targetInstallDirectory)
        {
            string manifestPath = Path.Combine(packageDirectory, UpdateConstants.ManifestFileName);
            string sigPath = Path.Combine(packageDirectory, UpdateConstants.ManifestSigFileName);
            string zipPath = Path.Combine(packageDirectory, UpdateConstants.ZipFileName);

            void CleanupFailedPackage()
            {
                DeleteFileIfExists(manifestPath);
                DeleteFileIfExists(sigPath);
                DeleteFileIfExists(zipPath);
            }

            if (!File.Exists(manifestPath) || !File.Exists(sigPath) || !File.Exists(zipPath))
            {
                return false;
            }

            // 1. Verify detached manifest.json.sig
            string manifestJson = File.ReadAllText(manifestPath);
            string manifestSig = File.ReadAllText(sigPath);
            byte[] manifestBytes = Encoding.UTF8.GetBytes(manifestJson);

            if (!VerifyData(manifestBytes, manifestSig))
            {
                CleanupFailedPackage();
                return false; // Manifest has been tampered with
            }

            // 2. Deserialize UpdateManifest
            UpdateManifest manifest = JsonConvert.DeserializeObject<UpdateManifest>(manifestJson);
            if (manifest == null) return false;

            // 3. Verify overall Zip package signature
            byte[] zipBytes = File.ReadAllBytes(zipPath);
            if (!VerifyData(zipBytes, manifest.ZipSignature))
            {
                CleanupFailedPackage();
                PurgeTempUpdateZips();
                return false;
            }

            // 4. Extract zip to a temporary directory so we can inspect and verify inner files
            string tempExtractPath = Path.Combine(packageDirectory, "temp_extracted_" + Guid.NewGuid().ToString());

            try
            {
                Directory.CreateDirectory(tempExtractPath);
                ZipFile.ExtractToDirectory(zipPath, tempExtractPath);

                string fullExePath = Process.GetCurrentProcess().MainModule.FileName;
                string currentExeName = Path.GetFileName(fullExePath);

                string sourceUpdaterPath = string.Empty;
                string sourceUpdaterConfigPath = string.Empty;
                string sourceWpfPath = string.Empty;
                string sourceWpfConfigPath = string.Empty;

                // 5. Verify individual file signatures inside the extracted contents
                foreach (var fileSig in manifest.Files)
                {
                    string targetFilePath = Path.Combine(tempExtractPath, fileSig.FileName);
                    if (!VerifyFileSignature(targetFilePath, fileSig))
                    {
                        CleanupFailedPackage();
                        DeleteFolderIfExists(tempExtractPath);
                        PurgeTempUpdateZips();
                        return false;
                    }

                    // Capture paths for deployment
                    if (fileSig.FileName.Equals("ruler.updater.exe", StringComparison.OrdinalIgnoreCase))
                    {
                        sourceUpdaterPath = targetFilePath;
                    }
                    if (fileSig.FileName.Equals("ruler.updater.exe.config", StringComparison.OrdinalIgnoreCase))
                    {
                        sourceUpdaterConfigPath = targetFilePath;
                    }
                    if (fileSig.FileName.Equals(currentExeName, StringComparison.OrdinalIgnoreCase))
                    {
                        sourceWpfPath = targetFilePath;
                    }
                    if (fileSig.FileName.Equals(currentExeName + ".config", StringComparison.OrdinalIgnoreCase))
                    {
                        sourceWpfConfigPath = targetFilePath;
                    }
                }

                // Ensure required files were present in the verified manifest
                if (string.IsNullOrEmpty(sourceUpdaterPath) || string.IsNullOrEmpty(sourceWpfPath) || string.IsNullOrEmpty(sourceUpdaterConfigPath)|| string.IsNullOrEmpty(sourceWpfConfigPath)) 
                {
                    return false;
                }

                // 6. Stage the new updater (since the updater isn't currently running, we can copy it directly)
                Directory.CreateDirectory(targetInstallDirectory);
                string targetUpdaterPath = Path.Combine(targetInstallDirectory, "ruler.updater.exe");
                string targetUpdaterConfigPath = Path.Combine(targetInstallDirectory, "ruler.updater.exe.config");

                File.Copy(sourceUpdaterPath, targetUpdaterPath, overwrite: true);
                File.Copy(sourceUpdaterConfigPath, targetUpdaterConfigPath, overwrite: true);

                // 7. Verify the updater was copied successfully and check size
                if (!File.Exists(targetUpdaterPath))
                {
                    return false;
                }

                FileInfo sourceInfo = new FileInfo(sourceUpdaterPath);
                FileInfo targetInfo = new FileInfo(targetUpdaterPath);
                if (sourceInfo.Length != targetInfo.Length)
                {
                    return false;
                }

                // 8. Prepare arguments and launch the updater to replace the running WPF app
                int currentProcessId = Process.GetCurrentProcess().Id;
                string arguments = $"\"{sourceWpfPath}\" \"{sourceWpfConfigPath}\" \"{targetInstallDirectory}\"\"{currentExeName}\" ";

                Process.Start(new ProcessStartInfo
                {
                    FileName = targetUpdaterPath,
                    Arguments = arguments,
                    UseShellExecute = true
                });

                // 9. Immediately shut down the main app to release file locks
                Environment.Exit(0);
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                // Clean up the temporary extraction folder (if exit didn't occur first)
                CleanupFailedPackage();
                DeleteFolderIfExists(tempExtractPath);
                PurgeTempUpdateZips();
            }
        }

        private static void DeleteFileIfExists(string path)
        {
            if (File.Exists(path))
            {
                try { File.Delete(path); } catch { }
            }
        }

        private static void DeleteFolderIfExists(string path)
        {
            if (Directory.Exists(path))
            {
                try
                {
                    Directory.Delete(path, true);
                }
                catch (Exception)
                {
                }
            }
        }
        private static void PurgeTempUpdateZips()
        {
            try
            {
                string tempPath = Path.GetTempPath();
                // Use the shared constant pattern or base name for cleanup
                string searchPattern = $"*{UpdateConstants.ZipFileName}";
                string[] leftoverZips = Directory.GetFiles(tempPath, searchPattern);
                foreach (string zip in leftoverZips)
                {
                    try
                    {
                        File.Delete(zip);
                    }
                    catch
                    {
                        // Suppress individual file deletion locks if open elsewhere 
                    }
                }
            }
            catch
            {
                // Suppress errors during global temp scan
            }
        }
    }
}


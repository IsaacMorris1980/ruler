using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using Octokit;


using ProductHeaderValue = Octokit.ProductHeaderValue;

namespace Ruler.Shared.Services.Update
{
    public class UpdateService
    {
        private readonly GitHubClient _githubClient;
        private readonly string _currentVersion;
        private readonly string _repoOwner;
        private readonly string _repoName;
        private readonly string _expectedPublisherName;

        public UpdateService(string currentVersion, string repoOwner, string repoName, string expectedPublisherName, string appName = "RulerApp")
        {
            _currentVersion = currentVersion;
            _repoOwner = repoOwner;
            _repoName = repoName;
            _expectedPublisherName = expectedPublisherName;

            // Octokit requires a unique User-Agent header initialization
            _githubClient = new GitHubClient(new ProductHeaderValue(appName));
        }

        public class GitHubUpdateDetails
        {
            public string NewVersion { get; set; }
            public string DownloadUrl { get; set; }
            public string ExpectedHash { get; set; }
        }

        // 1. CHECK FOR LATEST RELEASE VIA OCTOKIT
        public async Task<GitHubUpdateDetails> CheckForUpdatesAsync()
        {
            try
            {
                var latestRelease = await _githubClient.Repository.Release.GetLatest(_repoOwner, _repoName);

                string cleanTagName = latestRelease.TagName.TrimStart('v', 'V');

                Version latestVersion = Version.Parse(cleanTagName);
                Version currentRunningVersion = Version.Parse(_currentVersion);

                if (latestVersion > currentRunningVersion)
                {
                    // Find the primary .exe setup asset in the release bundle
                    var setupAsset = latestRelease.Assets.FirstOrDefault(a => a.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));
                    if (setupAsset == null) return null;

                    string expectedHash = ExtractHashFromReleaseNotes(latestRelease.Body);

                    return new GitHubUpdateDetails
                    {
                        NewVersion = cleanTagName,
                        DownloadUrl = setupAsset.BrowserDownloadUrl,
                        ExpectedHash = expectedHash
                    };
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[UpdateService] GitHub Octokit check failed: " + ex.Message);
            }

            return null;
        }

        // 2. DOWNLOAD PIPELINE
        public async Task<bool> ProcessUpdateAsync(GitHubUpdateDetails update)
        {
            if (update == null) return false;

            string tempDir = Path.Combine(Path.GetTempPath(), "RulerUpdates");
            if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir);

            string targetFilePath = Path.Combine(tempDir, string.Format("RulerSetup_{0}.exe", update.NewVersion));

            PurgeOldUpdateArtifacts(tempDir, targetFilePath);

            // Download using standard HttpClient (Supported natively in .NET 4.5+)
            using (var http = new HttpClient())
            {
                try
                {
                    using (var response = await http.GetAsync(update.DownloadUrl, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();
                        using (var streamToRead = await response.Content.ReadAsStreamAsync())
                        using (var streamToWrite = File.Open(targetFilePath, System.IO.FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            await streamToRead.CopyToAsync(streamToWrite);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[UpdateService] Download streaming failed: " + ex.Message);
                    return false;
                }
            }

            // SHA-256 Checksum Check
            if (!string.IsNullOrEmpty(update.ExpectedHash) && !VerifyFileHash(targetFilePath, update.ExpectedHash))
            {
                PurgeFileSilently(targetFilePath);
                throw new CryptographicException("Download corrupted: GitHub SHA-256 string mismatch.");
            }

            // Authenticode Verification
            if (!VerifyAuthenticodeSignature(targetFilePath, _expectedPublisherName))
            {
                PurgeFileSilently(targetFilePath);
                throw new System.Security.SecurityException("Untrusted application signature block on target executable package.");
            }

            LaunchInstaller(targetFilePath);
            return true;
        }

        // 3. SHA-256 ALGORITHM
        private bool VerifyFileHash(string filePath, string expectedHash)
        {
            using (var sha256 = SHA256.Create())
            using (var stream = File.OpenRead(filePath))
            {
                byte[] hashBytes = sha256.ComputeHash(stream);
                string calculatedHash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);

                return string.Equals(calculatedHash, expectedHash, StringComparison.OrdinalIgnoreCase);
            }
        }

        // 4. SIGNATURE VERIFICATION
        private bool VerifyAuthenticodeSignature(string filePath, string expectedPublisher)
        {
            try
            {
                // Supported in .NET Framework 4.8 via X509Certificate assembly references
                var certificate = X509Certificate.CreateFromSignedFile(filePath);
                using (var signingCert = new X509Certificate2(certificate))
                {
                    var chain = new X509Chain();
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;

                    if (!chain.Build(signingCert)) return false;

                    return signingCert.Subject.IndexOf(expectedPublisher, StringComparison.OrdinalIgnoreCase) >= 0;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[UpdateService] Authenticode crash: " + ex.Message);
                return false;
            }
        }

        // 5. BACKPORTED STRING PARSER LOGIC
        private string ExtractHashFromReleaseNotes(string bodyText)
        {
            if (string.IsNullOrEmpty(bodyText)) return string.Empty;

            using (var reader = new StringReader(bodyText))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // .NET 4.8 doesn't have the String.Contains(..., StringComparison) overload
                    if (line.IndexOf("SHA256:", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        // Backported split replacement logic for old C# variants
                        string[] parts = line.Split(new string[] { "SHA256:" }, StringSplitOptions.None);
                        if (parts.Length > 1)
                        {
                            return parts[1].Trim();
                        }
                    }
                }
            }
            return string.Empty;
        }

        private void PurgeOldUpdateArtifacts(string directoryPath, string currentTarget)
        {
            try
            {
                foreach (var file in Directory.GetFiles(directoryPath))
                {
                    if (file != currentTarget) File.Delete(file);
                }
            }
            catch { }
        }

        private void PurgeFileSilently(string filePath)
        {
            try { if (File.Exists(filePath)) File.Delete(filePath); } catch { }
        }

        private void LaunchInstaller(string filePath)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true,
                Verb = "runas"
            };
            Process.Start(startInfo);
        }
    }
}

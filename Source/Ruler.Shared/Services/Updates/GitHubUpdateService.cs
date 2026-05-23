using Newtonsoft.Json;

using Ruler.Contracts.Models;
using Ruler.Contracts.Services.Updates;
using Ruler.Contracts.Services.Verification;
using Ruler.Shared.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Services.Updates
{
    public class GithubUpdateService : IGithubUpdateService
    {
        private readonly IActiveKeyVerifier _keyVerifier;
        private readonly string _masterPublicKey;
        private readonly string _activePublicKey;

        // 🚀 Using your real domain model now
        private GitHubRelease _latestRelease;
        private string _repoOwner;
        private string _repoName;

        public GithubUpdateService(IActiveKeyVerifier keyVerifier, string activePublicKeyXml, string masterPublicKeyXml)
        {
            _keyVerifier = keyVerifier ?? throw new ArgumentNullException(nameof(keyVerifier));
            _activePublicKey = activePublicKeyXml ?? throw new ArgumentNullException(nameof(activePublicKeyXml));
            _masterPublicKey = masterPublicKeyXml ?? throw new ArgumentNullException(nameof(masterPublicKeyXml));

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | (SecurityProtocolType)3072; // TLS 1.3
        }

        /// <summary>
        /// Queries the GitHub API and uses native serialization models to inspect versions.
        /// </summary>
        public async Task<bool> CheckForUpdatesAsync(string currentVersion, string repositoryOwner, string repositoryName)
        {
            _repoOwner = repositoryOwner;
            _repoName = repositoryName;

            string apiUrl = $"https://api.github.com/repos/{_repoOwner}/{_repoName}/releases/latest";

            try
            {
                using (var webClient = new WebClient())
                {
                    webClient.Headers.Add("User-Agent", "RulerApp-Updater-Engine");

                    string jsonResponse = await webClient.DownloadStringTaskAsync(apiUrl);

                    // 🚀 CLEANUP: Swapped out custom string manipulation for clean deserialization
                    _latestRelease = JsonConvert.DeserializeObject<GitHubRelease>(jsonResponse);

                    if (_latestRelease == null || string.IsNullOrEmpty(_latestRelease.TagName))
                        return false;

                    // Skip or include prereleases based on your workflow preferences
                    if (_latestRelease.IsPrerelease)
                        return false;

                    var current = new Version(currentVersion.Replace("v", ""));
                    var latest = new Version(_latestRelease.TagName.Replace("v", ""));

                    return latest > current;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Downloads the actual assembly payload alongside its corresponding cryptographic digital signature asset.
        /// </summary>
        public async Task<bool> DownloadAndVerifyUpdateAsync(string destinationDirectory)
        {
            if (_latestRelease == null || _latestRelease.Assets == null) return false;

            string binaryDownloadUrl = null;
            string signatureDownloadUrl = null;
            string binaryFileName = null;

            // 🚀 Safe evaluation iterating through your exact model structures
            foreach (var asset in _latestRelease.Assets)
            {
                if (asset.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
                    asset.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    binaryDownloadUrl = asset.DownloadUrl; // 🚀 Uses your model's "DownloadUrl" property mapping
                    binaryFileName = asset.Name;
                }
                else if (asset.Name.EndsWith(".exe.sig", StringComparison.OrdinalIgnoreCase) ||
                         asset.Name.EndsWith(".zip.sig", StringComparison.OrdinalIgnoreCase))
                {
                    signatureDownloadUrl = asset.DownloadUrl;
                }
            }

            if (binaryDownloadUrl == null || signatureDownloadUrl == null)
            {
                return false; // Incomplete package deployment state
            }

            string localBinaryPath = Path.Combine(destinationDirectory, binaryFileName);
            string localSignaturePath = Path.Combine(destinationDirectory, binaryFileName + ".sig");

            try
            {
                if (!Directory.Exists(destinationDirectory)) Directory.CreateDirectory(destinationDirectory);

                using (var webClient = new WebClient())
                {
                    webClient.Headers.Add("User-Agent", "RulerApp-Updater-Engine");

                    await webClient.DownloadFileTaskAsync(binaryDownloadUrl, localBinaryPath);
                    await webClient.DownloadFileTaskAsync(signatureDownloadUrl, localSignaturePath);

                    byte[] signatureBytes = File.ReadAllBytes(localSignaturePath);

                    bool isVerified = _keyVerifier.VerifyFileSignature(
                        localBinaryPath,
                        signatureBytes,
                        _activePublicKey,
                        _masterPublicKey
                    );

                    if (File.Exists(localSignaturePath)) File.Delete(localSignaturePath);

                    if (!isVerified)
                    {
                        if (File.Exists(localBinaryPath)) File.Delete(localBinaryPath);
                        return false;
                    }

                    return true;
                }
            }
            catch (Exception)
            {
                if (File.Exists(localBinaryPath)) File.Delete(localBinaryPath);
                if (File.Exists(localSignaturePath)) File.Delete(localSignaturePath);
                return false;
            }
        }
        public async Task<string> FetchLatestReleaseDataAsync(string repositoryOwner, string repositoryName)
        {
            string apiUrl = $"https://api.github.com/repos/{repositoryOwner}/{repositoryName}/releases/latest";
            using (var webClient = new WebClient())
            {
                webClient.Headers.Add("User-Agent", "RulerApp-Updater-Engine");
                return await webClient.DownloadStringTaskAsync(apiUrl);
            }
        }
    }
}

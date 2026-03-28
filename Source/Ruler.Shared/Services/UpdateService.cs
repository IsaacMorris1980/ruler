using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Ruler.Wpf.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Ruler.Wpf.Services
{
    public class UpdateService : IUpdateService
    {
        private bool _isUpdateAvailable;
        private bool _isBetaUpdateAvailable;
        private UpdateVersion _latestVersion;
        private GitHubRelease _latestRelease;
        private GitHubRelease _latestBetaRelease;
        public bool UpdateAvailable { get
                {
                return _isUpdateAvailable;
            }
        }
        public bool BetaUpdateAvailable
        {
            get
            {
                return _isBetaUpdateAvailable;
            }
        }
        public UpdateVersion UserWantsUpdate { get; set; }
        public bool UpdateDownloaded { get; }

        public async Task CheckForUpdates()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            using (var client = new HttpClient())
            {
                // 2. GitHub API MUST have a User-Agent header
                client.DefaultRequestHeaders.Add("User-Agent", "MyUpdaterApp");

                string apiUrl = "https://api.github.com/repos/andrijac/ruler/releases";

                try
                {
                    // 3. Fetch the JSON string
                    string responseBody = await client.GetStringAsync(apiUrl);

                    // 4. Parse the JSON
                    var releases = JsonConvert.DeserializeObject<List<GitHubRelease>>(responseBody);

                    _latestRelease = releases.FirstOrDefault(r => !r.TagName.Contains("Beta"));
                    _latestBetaRelease = releases.FirstOrDefault(r => r.TagName.Contains("Beta"));
                    Version currentVersion = Assembly.GetEntryAssembly().GetName().Version;
                    if (_latestRelease != null)
                    {
                        Version latestVersion = new Version(_latestRelease.TagName.TrimStart('v'));
                        if (latestVersion > currentVersion)
                        {
                            _isUpdateAvailable = true;

                        }

                    }
                    else if (_latestBetaRelease != null)
                    {

                    }

                    // Look for a .zip file in the assets list
                    foreach (var asset in release.assets)
                    {
                        string fileName = asset.name;
                        if (fileName.EndsWith(".zip"))
                        {
                            downloadUrl = asset.browser_download_url;
                            break;
                        }
                    }

                    Console.WriteLine($"Found Tag: {tagName}");
                    Console.WriteLine($"Download: {downloadUrl}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
        }

        public bool DownloadUpdate(string type)
        {
            if (string.Equals(type,"beta",StringComparison.OrdinalIgnoreCase))
            {

            }
        }

        public void InstallUpdate(string type)
        {
            if (string.Equals(type, "beta", StringComparison.OrdinalIgnoreCase))
            {

            }
        }
    }
}

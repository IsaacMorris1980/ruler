using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ruler.Shared.Models;

namespace Ruler.Shared.Interfaces
{
    public interface IUpdateService
    {
        Task<UpdatePackageInfo> GetLatestGitHubAssetUrlsAsync(string owner, string repo);
        Task<(bool available, string latestVersion)> CheckForUpdateAsync(string manifestUrl, string currentVersion);
        Task DownloadAndApplyUpdateAsync(string manifestUrl, string sigUrl, string zipUrl, string targetDir);
    }
}

using Ruler.Contracts.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Contracts.Services.Updates
{
    public interface IGithubUpdateService
    {
        /// <summary>
        /// Checks the target GitHub repository for a newer release version.
        /// </summary>
        Task<bool> CheckForUpdatesAsync(string currentVersion, string repositoryOwner, string repositoryName);

        /// <summary>
        /// Downloads the latest release binary, verifies its signature, and triggers the installer/updater.
        /// </summary>
        /// <param name="destinationDirectory">The local directory where the update package should be staged.</param>
        Task<bool> DownloadAndVerifyUpdateAsync(string destinationDirectory);

        /// <summary>
        /// Queries the GitHub API for the latest release asset catalog listings.
        /// </summary>
        Task<string> FetchLatestReleaseDataAsync(string repositoryOwner, string repositoryName);
    }
}

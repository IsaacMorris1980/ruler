using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ruler.Contracts.Models;

namespace Ruler.Contracts.Services.Updates
{
    public interface IUpdateService
    {
        /// <summary>
        /// Fires continuously to report the percentage-based download progress of the installer.
        /// </summary>
        event Action<int> DownloadProgressChanged;

        /// <summary>
        /// Fires when the internal pipeline step or status message changes.
        /// </summary>
        event Action<string> UpdateStatusChanged;

        /// <summary>
        /// Checks the repository for a signed update manifest, validates its signature,
        /// and applies the update package if a newer version is found.
        /// </summary>
        Task<bool> RunUpdateLifecycleAsync(string currentVersion, string repoOwner, string repoName);
      
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Contracts.Services.Updates
{
    public interface IHttpDownloadService
    {
        /// <summary>
        /// Fires continuously to report the percentage-based download progress.
        /// </summary>
        event Action<int> ProgressChanged;

        /// <summary>
        /// Fires when the download has successfully completed.
        /// </summary>
        event Action DownloadCompleted;

        /// <summary>
        /// Downloads a file asynchronously from a remote URL to a local destination path.
        /// </summary>
        Task DownloadFileAsync(string fileUrl, string destinationPath);
    }
}

using System.Threading.Tasks;

namespace Ruler.Contracts.Services.Updates
{
    public interface IUpdateService
    {
        /// <summary>
        /// Indicates whether a validated, signed newer version is available for installation.
        /// </summary>
        bool UpdateAvailable { get; }

        /// <summary>
        /// Checkpoint 1: Asynchronously contacts the update server, fetches the manifest, 
        /// and cryptographically authenticates its signature before flagging availability.
        /// </summary>
        Task CheckForUpdatesAsync();

        /// <summary>
        /// Checkpoint 2: Downloads the remote distribution archive payload (.zip) 
        /// and verifies its physical hash against the master public certificate block.
        /// </summary>
        Task<bool> DownloadUpdateAsync();

        /// <summary>
        /// Checkpoint 3: Unpacks the verified distribution payload to a temporary directory,
        /// validates individual executable signatures, and switches execution control to the worker.
        /// </summary>
        void InstallUpdate();
    }
}


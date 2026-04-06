using System.Threading.Tasks;

namespace Ruler.Shared
{
    public interface IUpdateService
    {
        bool UpdateAvailable { get; }
        UpdateVersion UserWantsUpdate { get; set; }
        bool UpdateDownloaded { get; }
        Task CheckForUpdates();
        Task<bool> DownloadUpdateAsync();
         void InstallUpdate();
    }
}

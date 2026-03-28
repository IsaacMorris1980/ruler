using Ruler.Wpf.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Interfaces
{
    public interface IUpdateService
    {
        bool UpdateAvailable { get; }
        UpdateVersion UserWantsUpdate { get; set; }
        bool UpdateDownloaded { get; }
        void CheckForUpdates();
        bool DownloadUpdate();
         void InstallUpdate();
    }
}

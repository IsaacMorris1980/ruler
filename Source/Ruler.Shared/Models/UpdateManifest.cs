using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Models
{
    public class UpdateManifest
    {
        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("releaseDate")]
        public string ReleaseDate { get; set; }

        [JsonProperty("changelog")]
        public string Changelog { get; set; }

        [JsonProperty("packageHash")]
        public string PackageHash { get; set; } // SHA256 of the ZIP file

        [JsonProperty("files")]
        public List<FileEntry> Files { get; set; } // Individual EXE/DLL hashes
    }
}

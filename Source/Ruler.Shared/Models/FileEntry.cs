using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Models
{
    public class FileEntry
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("FileName")]
        public string FileName { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("thumbprint")]
        public string Thumbprint { get; set; }
    }
}

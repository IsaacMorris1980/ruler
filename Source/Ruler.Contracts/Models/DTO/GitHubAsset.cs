using Newtonsoft.Json;

namespace Ruler.Contracts.Models.DTO
{
    public class GitHubAsset
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("browser_download_url")]
        public string DownloadUrl { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }
    }
}

using Newtonsoft.Json;
using System.Collections.Generic;

namespace Ruler.Contracts.Models.DTO
{
    public class GitHubRelease
    {
        [JsonProperty("tag_name")]
        public string TagName { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("prerelease")]
        public bool IsPrerelease { get; set; }

        [JsonProperty("body")]
        public string Description { get; set; }

        [JsonProperty("assets")]
        public List<GitHubAsset> Assets { get; set; }
    }
}

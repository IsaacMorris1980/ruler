using Newtonsoft.Json;

namespace Ruler.Contracts.Models.DTO
{
    public class FileEntry
    {
        public string Name { get; set; }

        public string Hash { get; set; }

        public string FileName { get; set; }

        public string Version { get; set; }

        public string Thumbprint { get; set; }
       
        public string DownloadUrl { get; set; }
    }
}


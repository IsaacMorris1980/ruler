using Newtonsoft.Json;
using System.Collections.Generic;

namespace Ruler.Contracts.Models.DTO
{
    public class UpdateManifest
    {
        
        public string Version { get; set; }

   
        public string ReleaseDate { get; set; }

       
        public string Changelog { get; set; }

        
        public string ZipSignature { get; set; } // SHA256 of the ZIP file
       
        public string ZipPackageName { get; set; }

       
        public List<FileEntry> Files { get; set; } // Individual EXE/DLL hashes
    }
}

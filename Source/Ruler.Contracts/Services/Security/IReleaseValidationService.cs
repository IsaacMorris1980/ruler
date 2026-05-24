using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Contracts.Services.Security
{
    public interface IReleaseValidationService
    {
        /// <summary>
        /// Checkpoint 1: Cryptographically validates the raw manifest JSON string 
        /// against a detached .sig text file using the Active Key or Master Backup Key.
        /// </summary>
        bool VerifyStringSignature(string data, string base64Signature);

        /// <summary>
        /// Checkpoints 2 & 3: Cryptographically validates physical bytes of a local file 
        /// (.zip or .exe binaries) using the Active Key or Master Backup Key.
        /// </summary>
        bool VerifyFileSignature(string filePath, string base64Signature);

        /// <summary>
        /// Guard against Zip Slip / Directory Traversal vulnerability overrides during extraction.
        /// </summary>
        bool IsPathSafe(string basePath, string targetFilePath);
    }
}

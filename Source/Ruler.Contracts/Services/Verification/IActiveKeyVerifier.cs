using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Contracts.Services.Verification
{
    public interface IActiveKeyVerifier
    {
        /// <summary>
        /// Validates a raw string payload (like a configuration or licensing JSON string) 
        /// against the current known Active and Master Public Keys.
        /// </summary>
        bool VerifyPayloadString(string plainTextData, string signature, string activePublicKeyXml, string masterPublicKeyXml);

        /// <summary>
        /// Validates an entire application file (like an update executable or patch) 
        /// against the current known Active and Master Public Keys.
        /// </summary>
        bool VerifyFileSignature(string filePath, byte[] signature, string activePublicKeyXml, string masterPublicKeyXml);
    }
}

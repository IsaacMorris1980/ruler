using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Contracts.Services.Verification
{
    public interface IMasterFallbackVerifier
    {
        /// <summary>
        /// Explicitly bypasses active keys to verify a raw text payload signature 
        /// directly against the long-term Master Public Key.
        /// </summary>
        bool VerifyPayloadWithMasterKey(string plainTextData, string signature, string masterPublicKeyXml);

        /// <summary>
        /// Explicitly bypasses active keys to verify a physical file signature 
        /// directly against the long-term Master Public Key.
        /// </summary>
        bool VerifyFileWithMasterKey(string filePath, byte[] signature, string masterPublicKeyXml);
    }
}

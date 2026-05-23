using Ruler.Contracts.Services;
using Ruler.Contracts.Services.Verification;

using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Services.Security.Verification
{
    public class MasterFallbackVerifier : IMasterFallbackVerifier
    {
        private readonly ISecurityService _securityService;

        // Injected automatically via Microsoft Dependency Injection
        public MasterFallbackVerifier(ISecurityService securityService)
        {
            _securityService = securityService ?? throw new ArgumentNullException(nameof(securityService));
        }

        /// <summary>
        /// Validates text data using exclusively the Master Public Key. 
        /// Useful for emergency configuration overrides or key-rotation actions.
        /// </summary>
        public bool VerifyPayloadWithMasterKey(string plainTextData, string signature, string masterPublicKeyXml)
        {
            if (string.IsNullOrEmpty(plainTextData) || string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(masterPublicKeyXml))
            {
                return false;
            }

            // We pass null for the active key parameter so the underlying security engine 
            // relies entirely on the Master Key pipeline evaluation
            return _securityService.VerifyData(plainTextData, signature, null, masterPublicKeyXml);
        }

        /// <summary>
        /// Validates vital binaries (like critical recovery patches or download manifests) 
        /// directly against the root Master Public Key.
        /// </summary>
        public bool VerifyFileWithMasterKey(string filePath, byte[] signature, string masterPublicKeyXml)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath) || signature == null || signature.Length == 0 || string.IsNullOrEmpty(masterPublicKeyXml))
            {
                return false;
            }

            // Execute explicit master-only file mapping validation
            return _securityService.VerifyFile(filePath, signature, null, masterPublicKeyXml);
        }
    }
}

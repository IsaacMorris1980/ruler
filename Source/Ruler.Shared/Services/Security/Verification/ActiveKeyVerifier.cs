using Ruler.Contracts.Services;
using Ruler.Contracts.Services.Verification;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Services.Security.Verification
{
    public class ActiveKeyVerifier : IActiveKeyVerifier
    {
        private readonly ISecurityService _securityService;

        // 🚀 Injected via Microsoft Dependency Injection automatically
        public ActiveKeyVerifier(ISecurityService securityService)
        {
            _securityService = securityService ?? throw new ArgumentNullException(nameof(securityService));
        }

        /// <summary>
        /// Verifies a text block/JSON data signature. 
        /// Falls back to the Master Public Key if the Active Key fails or is expired.
        /// </summary>
        public bool VerifyPayloadString(string plainTextData, string signature, string activePublicKeyXml, string masterPublicKeyXml)
        {
            if (string.IsNullOrEmpty(plainTextData) || string.IsNullOrEmpty(signature))
            {
                return false;
            }

            // Route directly to our security engine which handles the active-to-master cascade validation
            return _securityService.VerifyData(plainTextData, signature, activePublicKeyXml, masterPublicKeyXml);
        }

        /// <summary>
        /// Verifies a physical file's signature on disk. 
        /// Essential for validating downloaded updater EXEs or asset manifests before running them.
        /// </summary>
        public bool VerifyFileSignature(string filePath, byte[] signature, string activePublicKeyXml, string masterPublicKeyXml)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath) || signature == null || signature.Length == 0)
            {
                return false;
            }

            // Route directly to our security engine's file verification pipeline
            return _securityService.VerifyFile(filePath, signature, activePublicKeyXml, masterPublicKeyXml);
        }
    }
}

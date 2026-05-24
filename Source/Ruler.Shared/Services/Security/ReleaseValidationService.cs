using Ruler.Contracts.Services.Security;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Services.Security
{
    public class ReleaseValidationService : IReleaseValidationService
    {
        // Replace with your real public-only key strings from your deployment certificate assets
        private const string ACTIVE_PUBLIC_KEY_XML =
            "<RSAKeyValue><Modulus>v34A...CurrentActivePublicKeyHere...=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        private const string MASTER_PUBLIC_KEY_XML =
            "<RSAKeyValue><Modulus>z98X...PermanentMasterBackupKeyHere...=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        public bool VerifyStringSignature(string data, string base64Signature)
        {
            if (string.IsNullOrEmpty(data) || string.IsNullOrEmpty(base64Signature)) return false;

            // Chained Check 1: Attempt Verification using the active key
            if (ExecuteRsaVerification(data, base64Signature, ACTIVE_PUBLIC_KEY_XML))
            {
                return true;
            }

            // Chained Check 2: Fallback to verification via Master Backup Key
            System.Diagnostics.Debug.WriteLine("Active key verification failed. Attempting Master Backup Key verification...");
            return ExecuteRsaVerification(data, base64Signature, MASTER_PUBLIC_KEY_XML);
        }

        public bool VerifyFileSignature(string filePath, string base64Signature)
        {
            if (!File.Exists(filePath) || string.IsNullOrEmpty(base64Signature)) return false;

            // Chained Check 1: Try active key file verification
            if (ExecuteFileRsaVerification(filePath, base64Signature, ACTIVE_PUBLIC_KEY_XML))
            {
                return true;
            }

            // Chained Check 2: Try master backup key file verification
            System.Diagnostics.Debug.WriteLine("Active key file verification failed. Attempting Master Backup Key file verification...");
            return ExecuteFileRsaVerification(filePath, base64Signature, MASTER_PUBLIC_KEY_XML);
        }

        public bool IsPathSafe(string basePath, string targetFilePath)
        {
            if (string.IsNullOrEmpty(basePath) || string.IsNullOrEmpty(targetFilePath)) return false;
            try
            {
                string fullBasePath = Path.GetFullPath(basePath);
                string fullTargetPath = Path.GetFullPath(targetFilePath);
                return fullTargetPath.StartsWith(fullBasePath, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        #region Private Key Core Handlers
        private bool ExecuteRsaVerification(string data, string base64Signature, string publicKeyXml)
        {
            try
            {
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
                {
                    rsa.FromXmlString(publicKeyXml);
                    byte[] dataBytes = Encoding.UTF8.GetBytes(data);
                    byte[] signatureBytes = Convert.FromBase64String(base64Signature);

                    return rsa.VerifyData(dataBytes, CryptoConfig.MapNameToOID("SHA256"), signatureBytes);
                }
            }
            catch
            {
                return false;
            }
        }

        private bool ExecuteFileRsaVerification(string filePath, string base64Signature, string publicKeyXml)
        {
            try
            {
                if (!File.Exists(filePath)) return false;

                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
                {
                    rsa.FromXmlString(publicKeyXml);

                    // 1. Compute the SHA256 hash of the physical file stream
                    byte[] fileHash;
                    using (FileStream fs = File.OpenRead(filePath))
                    using (SHA256 sha256 = SHA256.Create())
                    {
                        fileHash = sha256.ComputeHash(fs); // Safe for large files
                    }

                    // 2. Decode the signature from Base64
                    byte[] signatureBytes = Convert.FromBase64String(base64Signature);

                    // 3. Verify using VerifyHash instead of VerifyData
                    return rsa.VerifyHash(fileHash, CryptoConfig.MapNameToOID("SHA256"), signatureBytes);
                }
            }
            catch
            {
                return false;
            }
        }
        #endregion
    }
}

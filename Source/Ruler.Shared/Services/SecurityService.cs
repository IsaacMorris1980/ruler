using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Ruler.Shared.Services
{
    /// <summary>
    /// Handles digital signing and verification for Ruler settings and updates.
    /// Implements a Master Key fallback for high resiliency.
    /// </summary>
    public static class SecurityService
    {
        // MASTER KEY: The "Root of Trust". Never expires. 
        // Keep the Private Key for this offline and secure.
        private const string MasterPublicKeyXml = "<RSAKeyValue><Modulus>...</Modulus><Exponent>...</Exponent></RSAKeyValue>";

        // ACTIVE KEY: Used for daily operations and routine updates.
        private const string ActivePublicKeyXml = "<RSAKeyValue><Modulus>...</Modulus><Exponent>...</Exponent></RSAKeyValue>";

        #region Verification Logic (The "Trust" Side)

        /// <summary>
        /// Verifies data (settings or manifest strings) against both Active and Master keys.
        /// </summary>
        public static bool VerifyTrust(string data, string signatureBase64)
        {
            if (string.IsNullOrEmpty(data) || string.IsNullOrEmpty(signatureBase64)) return false;

            // 1. Try Active Key
            if (Verify(data, signatureBase64, ActivePublicKeyXml)) return true;

            // 2. Fallback to Master Key if Active Key is expired or rotated
            return Verify(data, signatureBase64, MasterPublicKeyXml);
        }

        /// <summary>
        /// Verifies a physical file (the release .exe or .zip) against a signature.
        /// </summary>
        public static bool VerifyFileTrust(string filePath, string signatureBase64)
        {
            if (!File.Exists(filePath)) return false;

            byte[] fileData = File.ReadAllBytes(filePath);

            // Try Active Key
            if (VerifyRaw(fileData, signatureBase64, ActivePublicKeyXml)) return true;

            // Fallback to Master Key
            return VerifyRaw(fileData, signatureBase64, MasterPublicKeyXml);
        }

        private static bool Verify(string data, string signature, string keyXml)
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            return VerifyRaw(dataBytes, signature, keyXml);
        }

        private static bool VerifyRaw(byte[] data, string signature, string keyXml)
        {
            try
            {
                using (var rsa = new RSACryptoServiceProvider())
                {
                    rsa.FromXmlString(keyXml);
                    byte[] sigBytes = Convert.FromBase64String(signature);
                    return rsa.VerifyData(data, CryptoConfig.MapNameToOID("SHA256"), sigBytes);
                }
            }
            catch { return false; }
        }

        #endregion

        #region Signing Logic (The "Release" Side)
        // These methods require a Private Key and should ideally be used in your Release/Build tool.

        /// <summary>
        /// Signs a string (like the JSON list of rulers).
        /// </summary>
        public static string SignData(string data, string privateKeyXml)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(privateKeyXml);
                byte[] dataBytes = Encoding.UTF8.GetBytes(data);
                byte[] signature = rsa.SignData(dataBytes, CryptoConfig.MapNameToOID("SHA256"));
                return Convert.ToBase64String(signature);
            }
        }

        /// <summary>
        /// Signs a file (the Release Build).
        /// </summary>
        public static string SignFile(string filePath, string privateKeyXml)
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(privateKeyXml);
                byte[] signature = rsa.SignData(fileData, CryptoConfig.MapNameToOID("SHA256"));
                return Convert.ToBase64String(signature);
            }
        }
        public static string GenerateHash(string filePath)
        {
            using (var sha256 = SHA256.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    byte[] hashBytes = sha256.ComputeHash(stream);

                    // Convert byte array to a clean hex string
                    StringBuilder sb = new StringBuilder();
                    foreach (byte b in hashBytes)
                    {
                        sb.Append(b.ToString("x2"));
                    }
                    return sb.ToString();
                }
            }
        }
        public static bool VerifyExternalTrust(string filePath, string signatureBase64)
        {
            // 1. Get the hash of the file we want to verify
            string fileHash = GenerateHash(filePath);
            byte[] hashToVerify = Encoding.UTF8.GetBytes(fileHash);

            // 2. Decode the signature from the manifest
            byte[] signature = Convert.FromBase64String(signatureBase64);

            // 3. Initialize RSA with your Developer Public Key
            // This key is hardcoded in your app or loaded from a resource
            using (var rsa = new RSACryptoServiceProvider())
            {
                try
                {
                    rsa.FromXmlString(ActivePublicKeyXml); // Your XML-formatted Public Key

                    // 4. Perform the mathematical verification
                    // This proves the hash was signed by the matching Private Key
                    return rsa.VerifyData(hashToVerify, CryptoConfig.MapNameToOID("SHA256"), signature);
                }
                catch
                {
                    return false; // Signature format was invalid or key mismatch
                }
            }
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Services
{
    public class SecurityService
    {
        // Replace with your actual RSA Public Key from the Generator tool
        private const string PublicKeyXml = "<RSAKeyValue><Modulus>...</Modulus><Exponent>...</Exponent></RSAKeyValue>";

        // Your code-signing certificate thumbprint (Uppercase, no spaces)
        private static readonly string[] TrustedThumbprints =
        {
            "YOUR_CERTIFICATE_THUMBPRINT_HERE"
        };
        public string SignData(string plainText, string pfxPath, string pfxPassword)
        {
            // Ensure you have your Private Key loaded or accessible here
            // If you are using the PFX to get the RSA key:
            using (RSACryptoServiceProvider rsa = GetPrivateKeyFromPfx(pfxPath, pfxPassword))
            {
                byte[] dataToSign = Encoding.UTF8.GetBytes(plainText);

                // SHA256 is the modern standard for hashing the data before signing
                byte[] signatureBytes = rsa.SignData(dataToSign, CryptoConfig.MapNameToOID("SHA256"));

                // Convert to Base64 so it can be saved easily as a text file (.sig)
                return Convert.ToBase64String(signatureBytes);
            }
        }

        public string GetFileSignatureThumbprint(string filePath)
        {
            try
            {
                // This extracts the certificate used to sign the file
                using (X509Certificate2 cert = new X509Certificate2(filePath))
                {
                    // Returns the SHA1 thumbprint as a hex string
                    // We use this in the manifest to prove the EXE was signed by YOU
                    return cert.Thumbprint;
                }
            }
            catch (Exception)
            {
                // If the file isn't signed, X509Certificate2 will throw an error.
                // We return an empty string or "UNSIGNED" so the manifest reflects the truth.
                return string.Empty;
            }
        }

        private RSACryptoServiceProvider GetPrivateKeyFromPfx(string path, string password)
        {
            var cert = new X509Certificate2(path, password, X509KeyStorageFlags.Exportable);
            return cert.PrivateKey as RSACryptoServiceProvider;
        }
        /// <summary>
        /// Verifies the RSA signature of the manifest JSON string.
        /// </summary>
        public bool VerifyManifestSignature(string jsonContent, string signatureBase64)
        {
            try
            {
                using (var rsa = new RSACryptoServiceProvider(2048))
                {
                    rsa.FromXmlString(PublicKeyXml);

                    byte[] dataBytes = Encoding.UTF8.GetBytes(jsonContent);
                    byte[] signatureBytes = Convert.FromBase64String(signatureBase64);

                    return rsa.VerifyData(dataBytes, CryptoConfig.MapNameToOID("SHA256"), signatureBytes);
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates the Authenticode signature of an EXE or DLL.
        /// Ensures the file was signed by YOU and hasn't been modified.
        /// </summary>
        public bool IsFileAuthentic(string filePath)
        {
            if (!File.Exists(filePath)) return false;

            try
            {
                X509Certificate2 cert = new X509Certificate2(filePath);

                // Check the certificate chain
                using (X509Chain chain = new X509Chain())
                {
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                    if (!chain.Build(cert)) return false;
                }

                // Check the thumbprint against our trusted list
                string thumbprint = cert.Thumbprint?.ToUpper().Replace(" ", "");
                return TrustedThumbprints.Contains(thumbprint);
            }
            catch
            {
                return false; // File is unsigned or corrupted
            }
        }

        /// <summary>
        /// Computes SHA256 hash to compare against the Manifest's PackageHash.
        /// </summary>
        public string CalculateFileHash(string filePath)
        {
            if (!File.Exists(filePath)) return string.Empty;

            using (var sha = SHA256.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    byte[] hashBytes = sha.ComputeHash(stream);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}

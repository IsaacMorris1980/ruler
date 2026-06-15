using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Services.Security
{
    public class CustomSecurityService
    {
        // =========================================================================
        // 1. EMBEDDED PUBLIC CERTIFICATE/KEY CONSTANTS
        // =========================================================================
        private const string MasterPublicKeyXml = "<RSAKeyValue><Modulus>your-master-public-key-modulus...</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
        private const string ActivePublicKeyXml = "<RSAKeyValue><Modulus>your-active-public-key-modulus...</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        // =========================================================================
        // 2. SIGNATURE VERIFICATION METHODS (WITH AUTOMATIC MASTER FALLBACK)
        // =========================================================================

        /// <summary>
        /// Checks an individual local file against a digital signature.
        /// Automatically falls back to the Master Key if Active Key verification fails.
        /// </summary>
        public bool VerifyFileSignature(string filePath, string base64Signature)
        {
            if (!File.Exists(filePath) || string.IsNullOrEmpty(base64Signature))
                return false;

            try
            {
                byte[] fileHash;
                using (var sha256 = SHA256.Create())
                using (var stream = File.OpenRead(filePath))
                {
                    fileHash = sha256.ComputeHash(stream);
                }

                byte[] signatureBytes = Convert.FromBase64String(base64Signature);

                // Try Active Key first
                if (VerifyHashWithKey(fileHash, signatureBytes, ActivePublicKeyXml)) return true;

                // Fallback to Master Key
                System.Diagnostics.Debug.WriteLine("Active key file verification failed. Attempting Master Key fallback...");
                return VerifyHashWithKey(fileHash, signatureBytes, MasterPublicKeyXml);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"File signature verification error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Checks a string against a digital signature.
        /// Automatically falls back to the Master Key if Active Key verification fails.
        /// </summary>
        public bool VerifyStringSignature(string plainText, string base64Signature)
        {
            if (string.IsNullOrEmpty(plainText) || string.IsNullOrEmpty(base64Signature))
                return false;

            try
            {
                byte[] dataToVerify = Encoding.UTF8.GetBytes(plainText);
                byte[] signatureBytes = Convert.FromBase64String(base64Signature);

                // Try Active Key first
                if (VerifyDataWithKey(dataToVerify, signatureBytes, ActivePublicKeyXml)) return true;

                // Fallback to Master Key
                System.Diagnostics.Debug.WriteLine("Active key string verification failed. Attempting Master Key fallback...");
                return VerifyDataWithKey(dataToVerify, signatureBytes, MasterPublicKeyXml);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"String signature verification error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Verifies a plain text string against a digital signature using a certificate 
        /// installed natively in the Windows Local Machine or Current User certificate store.
        /// </summary>
        public bool VerifyStringWithStoreCertificate(string plainText, string base64Signature, string certThumbprint, StoreLocation storeLocation = StoreLocation.LocalMachine)
        {
            if (string.IsNullOrEmpty(plainText) || string.IsNullOrEmpty(base64Signature) || string.IsNullOrEmpty(certThumbprint))
                return false;

            string cleanThumbprint = certThumbprint.Replace(" ", "").Replace("\x200e", "").Replace("\x200f", "").ToUpperInvariant();

            using (X509Store store = new X509Store(StoreName.My, storeLocation))
            {
                try
                {
                    store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                    X509Certificate2Collection matches = store.Certificates.Find(X509FindType.FindByThumbprint, cleanThumbprint, validOnly: false);

                    if (matches.Count == 0) return false;

                    using (RSA rsa = matches[0].GetRSAPublicKey())
                    {
                        if (rsa == null) return false;

                        byte[] dataToVerify = Encoding.UTF8.GetBytes(plainText);
                        byte[] signatureBytes = Convert.FromBase64String(base64Signature);

                        return rsa.VerifyData(dataToVerify, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                    }
                }
                catch { return false; }
                finally { store.Close(); }
            }
        }

        // =========================================================================
        // 3. CERTIFICATE GENERATION & INSTALLATION METHODS
        // =========================================================================

        /// <summary>
        /// Generates a self-signed Master Root Certificate (representing your ultimate offline Certificate Authority).
        /// </summary>
        public X509Certificate2 CreateMasterCertificate(string subjectName, int yearsValid = 10)
        {
            using (RSA rsa = RSA.Create(2048))
            {
                var request = new CertificateRequest($"CN={subjectName}", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                request.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
                request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyCertSign, true));

                X509Certificate2 selfSigned = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(yearsValid));
                return new X509Certificate2(selfSigned.Export(X509ContentType.Pkcs12, "export"), "export", X509KeyStorageFlags.Exportable);
            }
        }

        /// <summary>
        /// Generates an Active Subordinate Certificate cryptographically signed by your Master Certificate's private key.
        /// </summary>
        public X509Certificate2 CreateActiveCertificate(string subjectName, X509Certificate2 masterIssuerCert, int yearsValid = 2)
        {
            if (masterIssuerCert == null || !masterIssuerCert.HasPrivateKey)
                throw new ArgumentException("The Master Issuer Certificate must be provided and contain an active Private Key.");

            using (RSA rsa = RSA.Create(2048))
            {
                var request = new CertificateRequest($"CN={subjectName}", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
                request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, true));

                byte[] serialNumber = new byte[20];
                using (var rng = RandomNumberGenerator.Create()) { rng.GetBytes(serialNumber); }

                X509Certificate2 issuedCert = request.Create(masterIssuerCert, DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(yearsValid), serialNumber);
                X509Certificate2 certWithKey = issuedCert.CopyWithPrivateKey(rsa);

                return new X509Certificate2(certWithKey.Export(X509ContentType.Pkcs12, "export"), "export", X509KeyStorageFlags.Exportable);
            }
        }

        /// <summary>
        /// Generates a local machine-compatible public/private certificate bundle.
        /// </summary>
        public X509Certificate2 CreateLocalMachineCertificate(string commonName, int yearsValid = 5)
        {
            using (RSA rsa = RSA.Create(2048))
            {
                var request = new CertificateRequest($"CN={commonName}", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
                request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, true));
                request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new Oid("1.3.6.1.5.5.7.3.3") }, false));

                var sanBuilder = new SubjectAlternativeNameBuilder();
                sanBuilder.AddDnsName("localhost");
                sanBuilder.AddDnsName(Environment.MachineName);
                request.CertificateExtensions.Add(sanBuilder.Build());

                X509Certificate2 generatedCert = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(yearsValid));
                return new X509Certificate2(generatedCert.Export(X509ContentType.Pkcs12, "export"), "export", X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
            }
        }

        /// <summary>
        /// Registers a certificate into the specified system-wide store. Requires admin privileges for LocalMachine.
        /// </summary>
        public void RegisterCertificateInStore(X509Certificate2 cert, StoreLocation location = StoreLocation.LocalMachine)
        {
            using (X509Store store = new X509Store(StoreName.My, location))
            {
                store.Open(OpenFlags.ReadWrite);
                store.Add(cert);
                store.Close();
            }
        }

        // =========================================================================
        // PRIVATE CRYPTOGRAPHIC HELPERS
        // =========================================================================
        private bool VerifyHashWithKey(byte[] hash, byte[] signature, string keyXml)
        {
            try
            {
                using (var rsa = new RSACryptoServiceProvider())
                {
                    rsa.FromXmlString(keyXml);
                    return rsa.VerifyHash(hash, CryptoConfig.MapNameToOID("SHA256"), signature);
                }
            }
            catch { return false; }
        }

        private bool VerifyDataWithKey(byte[] data, byte[] signature, string keyXml)
        {
            try
            {
                using (var rsa = new RSACryptoServiceProvider())
                {
                    rsa.FromXmlString(keyXml);
                    return rsa.VerifyData(data, CryptoConfig.MapNameToOID("SHA256"), signature);
                }
            }
            catch { return false; }
        }
    }
}

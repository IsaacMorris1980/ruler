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
    public static class SecurityService
    {
        private static string _activePublicKeyXml;
        private static string _masterPublicKeyXml;

        /// <summary>
        /// Initializes client-side public keys for update verification.
        /// </summary>
        public static void Initialize(string activePublicKeyXml, string masterPublicKeyXml)
        {
            _activePublicKeyXml = activePublicKeyXml;
            _masterPublicKeyXml = masterPublicKeyXml;
        }

        #region Tier 1: Master Certificate (Cold Storage PFX)

        /// <summary>
        /// Creates the 99-year Master Root certificate and saves it securely as a password-protected PFX file.
        /// Run this once on a secure machine and store the file offline.
        /// </summary>
        public static void CreateAndSaveMasterCertificate(string subjectName, string password, string destinationFilePath)
        {
            using (RSA rsa = RSA.Create(4096)) // 4096-bit for maximum security
            {
                var request = new CertificateRequest(
                    $"CN={subjectName}",
                    rsa,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1);

                request.CertificateExtensions.Add(
                    new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.KeyEncipherment, true));

                DateTimeOffset start = DateTimeOffset.UtcNow.AddDays(-1);
                DateTimeOffset end = start.AddYears(99);

                using (X509Certificate2 cert = request.CreateSelfSigned(start, end))
                {
                    byte[] pfxBytes = cert.Export(X509ContentType.Pfx, password);
                    File.WriteAllBytes(destinationFilePath, pfxBytes);
                }
            }
        }

        #endregion

        #region Tier 2: Active Certificate (Updates & Release Files)

        /// <summary>
        /// Creates an Active signing certificate (e.g., 2-year lifespan) and installs it into the Windows Local Machine store.
        /// Returns the thumbprint to be saved in Properties.Settings.
        /// </summary>
        public static string CreateAndStoreActiveCertificate(string subjectName)
        {
            using (RSA rsa = RSA.Create(2048))
            {
                var request = new CertificateRequest(
                    $"CN={subjectName}",
                    rsa,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1);

                request.CertificateExtensions.Add(
                    new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, false));

                DateTimeOffset start = DateTimeOffset.UtcNow.AddDays(-1);
                DateTimeOffset end = start.AddYears(2);

                using (X509Certificate2 cert = request.CreateSelfSigned(start, end))
                {
                    using (X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
                    {
                        store.Open(OpenFlags.ReadWrite);
                        store.Add(cert);
                    }
                    return cert.Thumbprint;
                }
            }
        }

        /// <summary>
        /// Retrieves the Active certificate from the Windows store using its thumbprint and signs a release file.
        /// </summary>
        public static string SignFileWithActiveCert(string filePath, string thumbprint)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException("File to sign not found.", filePath);

            string cleanThumb = thumbprint?.Replace(" ", "").ToUpper();
            using (var store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
            {
                store.Open(OpenFlags.ReadOnly);
                var certs = store.Certificates.Find(X509FindType.FindByThumbprint, cleanThumb, validOnly: false);

                if (certs.Count == 0) throw new Exception("Active signing certificate not found in LocalMachine store.");

                using (var rsa = certs[0].GetRSAPrivateKey())
                {
                    if (rsa == null) throw new CryptographicException("Private key unavailable for active certificate.");

                    byte[] fileData = File.ReadAllBytes(filePath);
                    byte[] signature = rsa.SignData(fileData, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                    return Convert.ToBase64String(signature);
                }
            }
        }

        #endregion

        #region Tier 3: Local Certificate (App Data / List of Rulers)

        /// <summary>
        /// Creates a local certificate stored in the CurrentUser store specifically for signing local application data.
        /// Returns the thumbprint (which can be saved in local settings/config).
        /// </summary>
        public static string CreateAndStoreLocalCertificate(string subjectName)
        {
            using (RSA rsa = RSA.Create(2048))
            {
                var request = new CertificateRequest(
                    $"CN={subjectName}",
                    rsa,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1);

                request.CertificateExtensions.Add(
                    new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, false));

                DateTimeOffset start = DateTimeOffset.UtcNow.AddDays(-1);
                DateTimeOffset end = start.AddYears(5);

                using (X509Certificate2 cert = request.CreateSelfSigned(start, end))
                {
                    using (X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
                    {
                        store.Open(OpenFlags.ReadWrite);
                        store.Add(cert);
                    }
                    return cert.Thumbprint;
                }
            }
        }

        /// <summary>
        /// Signs local app data (such as your serialized list of rulers) using the local user certificate.
        /// </summary>
        public static string SignAppData(string jsonData, string localCertThumbprint)
        {
            string cleanThumb = localCertThumbprint?.Replace(" ", "").ToUpper();
            using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
                store.Open(OpenFlags.ReadOnly);
                var certs = store.Certificates.Find(X509FindType.FindByThumbprint, cleanThumb, validOnly: false);

                if (certs.Count == 0) throw new Exception("Local app data certificate not found in CurrentUser store.");

                using (var rsa = certs[0].GetRSAPrivateKey())
                {
                    if (rsa == null) throw new CryptographicException("Private key unavailable for local app data certificate.");

                    byte[] dataBytes = Encoding.UTF8.GetBytes(jsonData);
                    byte[] signature = rsa.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                    return Convert.ToBase64String(signature);
                }
            }
        }

        /// <summary>
        /// Verifies local app data (such as your list of rulers) against tampering using the local certificate.
        /// </summary>
        public static bool VerifyAppData(string jsonData, string signatureBase64, string localCertThumbprint)
        {
            try
            {
                string cleanThumb = localCertThumbprint?.Replace(" ", "").ToUpper();
                using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
                {
                    store.Open(OpenFlags.ReadOnly);
                    var certs = store.Certificates.Find(X509FindType.FindByThumbprint, cleanThumb, validOnly: false);

                    if (certs.Count == 0) return false;

                    using (var rsa = certs[0].GetRSAPublicKey())
                    {
                        if (rsa == null) return false;

                        byte[] dataBytes = Encoding.UTF8.GetBytes(jsonData);
                        byte[] sigBytes = Convert.FromBase64String(signatureBase64);
                        return rsa.VerifyData(dataBytes, sigBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Client Verification Logic (Active & Master Fallback for Updates)

        public static bool VerifyFileTrust(string filePath, string signatureBase64)
        {
            if (!File.Exists(filePath)) return false;
            byte[] fileData = File.ReadAllBytes(filePath);

            if (VerifyRaw(fileData, signatureBase64, _activePublicKeyXml)) return true;
            return VerifyRaw(fileData, signatureBase64, _masterPublicKeyXml);
        }

        private static bool VerifyRaw(byte[] data, string signature, string keyXml)
        {
            try
            {
                if (string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(keyXml))
                    return false;

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
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Ruler.Shared.Models;

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
        private const string MasterPublicKeyXml = "<RSAKeyValue>\r\n  <Modulus>8pu6YlZzUk2yCNTAvhdtZAS7yqdIcXcxmRxFwlCtAxjdX4yOsUaGnuTqMK+XhRohAtA8b/IISvt6W2NxYh1BDbAesE02wO9loIYjU+uVHXGN4oM+bwsHxxDUOYfmYKKGAUybr2WkbP9KXdMZK4Ll4QqY8pbt+zOhnYIMptmYDTy0r1q4DyWVjPuE6lHim+GitHudISfXPUD40euo1eYWhxTApy/VgpKkpcko5fc4RqZGJFJMc2ma9t1PMgci/E0oLC1FNi+PLOfkDn6t3EblWhXpcUb9xDEkMeAXeg4ZXBwXAfty6MSGZMECE6XDau42wFtS5Vw/K1USDVojdIXTNQ==</Modulus> \r\n  <Exponent>AQAB</Exponent> \r\n  </RSAKeyValue>";

        // ACTIVE KEY: Used for daily operations and routine updates.
        private const string ActivePublicKeyXml = "<RSAKeyValue>\r\n  <Modulus>01RJtOcZzo17qnBi//L2OKzfr3tepSX6t+hqfXJR/YdBtPv7PwGrdY1V2B2h7qmu8CYAxT+7bKADq4Bm9ngoZwdq2vLK+JWOPbXiUHi7euvy+kBglpzKHCe+/QEfedEwkW/aEwHLj7MvdK4RDbX57wUHHXbLBzhMnr/JkzWNSOeaw91/BLlKYJmCppAisLLWmK05O03xN9G7Aj17rnpmQaEIJFVREPtfs3fEcdPPtZSxlcIpW7HySAWyV5CS28VxLH45GVYVpyD5v/tjEr5n/Uh/+gyGh+1MdCOHss/uKqKD1ge4m2gAxmPJ2oyh3xAQVa9jkV6v8p25XH6A4otuPQ==</Modulus> \r\n  <Exponent>AQAB</Exponent> \r\n  </RSAKeyValue>";

        #region Verification Logic (The "Trust" Side)

        /// <summary>
        /// Verifies data (settings or manifest strings) against both Active and Master keys.
        /// </summary>
        public static bool VerifyTrust(string data, string signatureBase64)
        {
            if (string.IsNullOrEmpty(data) || string.IsNullOrEmpty(signatureBase64)) return false;

           
            return Verify(data, signatureBase64);
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

        private static bool Verify(string data, string signature)
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            if (VerifyRaw(dataBytes, signature, ActivePublicKeyXml)) return true;

            // Fallback to Master Key
            return VerifyRaw(dataBytes, signature, MasterPublicKeyXml);
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

        #region Signing Logic (The "Release" Side)
        // These methods require a Private Key and should ideally be used in your Release/Build tool.

        /// <summary>
        /// Signs a string (like the JSON list of rulers).
        /// </summary>
        public static string SignData(string data)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(ActivePublicKeyXml);
                byte[] dataBytes = Encoding.UTF8.GetBytes(data);
                byte[] signature = rsa.SignData(dataBytes, CryptoConfig.MapNameToOID("SHA256"));
                return Convert.ToBase64String(signature);
            }
        }
        public static void CreateLongLifeMachineCertificate(string subjectName)
        {
            //RulerCertMetadata meta = new RulerCertMetadata()
            //{
            //    DeviceType = Shared.Enums.DevicesTypes.Desktop,
            //    IsRevoked = false,
            //    LastUsed = null
            //};
            using (RSA rsa = RSA.Create(2048))
            {
                var request = new CertificateRequest(
                    $"CN={subjectName}",
                    rsa,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1);

                // Standard usage for code signing/machine identification
                request.CertificateExtensions.Add(
                    new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, false));

                // Set start to yesterday (to avoid timezone "not yet valid" issues)
                DateTimeOffset start = DateTimeOffset.UtcNow.AddDays(-1);

                // Set end to 99 years from now
                DateTimeOffset end = start.AddYears(99);
                //meta.ExpirationDate = end.UtcDateTime;

                using (X509Certificate2 cert = request.CreateSelfSigned(start, end))
                {
                    //meta.Thumbprint = cert.Thumbprint;
                 
                    // Save to Local Machine Store (Requires Admin)
                    using (X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
                    {
                        store.Open(OpenFlags.ReadWrite);
                        store.Add(cert);
                        store.Close();
                    }

                    Console.WriteLine($"Permanent Cert Created. Valid until: {cert.NotAfter}");
                }
            }
        }
        /// <summary>
        /// Signs a file (the Release Build).
        /// </summary>
        public static string SignFile(string filePath)
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(ActivePublicKeyXml);
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
        public static string SignWithMachineKey(string data)
        {
            // Define the container parameters
            CspParameters cspParams = new CspParameters
            {
                // This name is the "ID" of your key. Windows will reuse it if it exists.
                KeyContainerName = "RulerApp_MachineKey",

                // This tells Windows to store the key in the hardware/OS level 
                // rather than the current user profile (better for persistence)
                Flags = CspProviderFlags.UseMachineKeyStore,

                // ProviderType 24 is required for SHA256 in .NET 4.8
                ProviderType = 24
            };

            using (var rsa = new RSACryptoServiceProvider(cspParams))
            {
                // We don't need to call ToXmlString. 
                // The key is already "saved" inside the named container above.
                byte[] dataBytes = Encoding.UTF8.GetBytes(data);
                byte[] signatureBytes = rsa.SignData(dataBytes, CryptoConfig.MapNameToOID("SHA256"));

                return Convert.ToBase64String(signatureBytes);
            }
        }
        public static bool VerifyWithMachineKey(string data, string signature)
        {
            CspParameters cspParams = new CspParameters
            {
                KeyContainerName = "RulerApp_MachineKey",
                Flags = CspProviderFlags.UseMachineKeyStore,
                ProviderType = 24
            };

            using (var rsa = new RSACryptoServiceProvider(cspParams))
            {
                byte[] dataBytes = Encoding.UTF8.GetBytes(data);
                byte[] signatureBytes = Convert.FromBase64String(signature);

                return rsa.VerifyData(dataBytes, CryptoConfig.MapNameToOID("SHA256"), signatureBytes);
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
        public static bool IsCertValidAndFunctional(string thumbprint)
        {
            if (string.IsNullOrEmpty(thumbprint)) return false;

            using (X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
            {
                store.Open(OpenFlags.ReadOnly);
                var certs = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
                store.Close();

                if (certs.Count == 0) return false; // Doesn't exist

                X509Certificate2 cert = certs[0];

                // 1. Check expiration (Is it currently active?)
                if (DateTime.Now < cert.NotBefore || DateTime.Now > cert.NotAfter)
                    return false;

                // 2. Check Private Key (Can we actually SIGN data with it?)
                if (!cert.HasPrivateKey)
                    return false;

                return true;
            }
        }
        //public static void RefreshHealthStatus(List<RulerCertMetadata> registry)
        //{
        //    using (X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
        //    {
        //        store.Open(OpenFlags.ReadOnly);

        //        foreach (var meta in registry)
        //        {
        //            // 1. Check logical revocation first (it's fastest)
        //            if (meta.IsRevoked)
        //            {
        //                meta.IsHealthy = false;
        //                continue;
        //            }

        //            // 2. Check physical existence and status
        //            var certs = store.Certificates.Find(X509FindType.FindByThumbprint, meta.Thumbprint, false);
        //            if (certs.Count > 0)
        //            {
        //                var cert = certs[0];

        //                // 3. Combine physical health with metadata
        //                bool isPhysicallyValid = cert.HasPrivateKey &&
        //                                         DateTime.Now >= cert.NotBefore &&
        //                                         DateTime.Now <= cert.NotAfter;

        //                meta.IsHealthy = isPhysicallyValid;
        //                meta.ExpirationDate = cert.NotAfter; // Update in case it changed
        //            }
        //            else
        //            {
        //                meta.IsHealthy = false;
        //            }
        //        }
        //        store.Close();
        //    }
        //}
        public static bool IsSecurityReady(string thumbprint)
        {
            if (string.IsNullOrEmpty(thumbprint)) return false;

            // Sanitize the thumbprint string to prevent matching errors
            string cleanThumb = thumbprint.Replace(" ", "").ToUpper();

            // Use LocalMachine for system-level or User for user-level storage
            using (X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
            {
                try
                {
                    store.Open(OpenFlags.ReadOnly);

                    // Look for the certificate (validOnly: false so we can inspect it ourselves)
                    X509Certificate2Collection certs = store.Certificates.Find(
                        X509FindType.FindByThumbprint,
                        cleanThumb,
                        validOnly: false);

                    // 1. Existence Check
                    if (certs.Count == 0) return false;

                    X509Certificate2 cert = certs[0];

                    // 2. Expiration Check (Is it currently in its valid window?)
                    bool isExpired = DateTime.Now < cert.NotBefore || DateTime.Now > cert.NotAfter;
                    if (isExpired) return false;

                    // 3. Capability Check (Does it have the private key required for signing?)
                    if (!cert.HasPrivateKey) return false;

                    return true;
                }
                catch (Exception)
                {
                    // If the store cannot be opened or queried, security is not ready
                    return false;
                }
                finally
                {
                    store.Close();
                }
            }
        }

      

        #endregion
    }
}

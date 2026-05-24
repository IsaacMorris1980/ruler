using Newtonsoft.Json.Linq;

using Ruler.Contracts.Services.Security;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Services.Security
{
    public class LocalValidationService : ILocalValidationService
    {
        public const string DefaultCertSubject = "CN=RulerLocalDataSigner";

        public bool IsConfigurationValid(string configJson)
        {
            if (string.IsNullOrWhiteSpace(configJson)) return false;
            try
            {
                JToken.Parse(configJson);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string SignData(string data, string certificateSubjectName)
        {
            if (string.IsNullOrEmpty(data)) return string.Empty;

            using (X509Certificate2 cert = FindMachineCertificate(certificateSubjectName))
            {
                if (cert == null || !cert.HasPrivateKey)
                {
                    throw new CryptographicException($"Machine certificate '{certificateSubjectName}' was not found or lacks a private key container.");
                }

                using (RSACryptoServiceProvider rsa = cert.PrivateKey as RSACryptoServiceProvider)
                {
                    if (rsa == null)
                        throw new CryptographicException("The machine certificate must support standard RSA configurations.");

                    byte[] dataBytes = Encoding.UTF8.GetBytes(data);
                    byte[] signatureBytes = rsa.SignData(dataBytes, CryptoConfig.MapNameToOID("SHA256"));
                    return Convert.ToBase64String(signatureBytes);
                }
            }
        }

        public bool VerifyData(string data, string base64Signature, string certificateSubjectName)
        {
            if (string.IsNullOrEmpty(data) || string.IsNullOrEmpty(base64Signature)) return false;

            try
            {
                using (X509Certificate2 cert = FindMachineCertificate(certificateSubjectName))
                {
                    if (cert == null) return false;

                    using (RSACryptoServiceProvider rsa = cert.PublicKey.Key as RSACryptoServiceProvider)
                    {
                        if (rsa == null) return false;

                        byte[] dataBytes = Encoding.UTF8.GetBytes(data);
                        byte[] signatureBytes = Convert.FromBase64String(base64Signature);
                        return rsa.VerifyData(dataBytes, CryptoConfig.MapNameToOID("SHA256"), signatureBytes);
                    }
                }
            }
            catch
            {
                return false; // Mismatches or formatting issues fail safe
            }
        }

        private X509Certificate2 FindMachineCertificate(string subjectName)
        {
            using (X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
            {
                store.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection collection = store.Certificates.Find(
                    X509FindType.FindBySubjectName,
                    subjectName,
                    validOnly: false);

                return collection.Count > 0 ? new X509Certificate2(collection[0]) : null;
            }
        }
    }
}

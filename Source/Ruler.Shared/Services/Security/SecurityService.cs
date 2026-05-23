using CERTENROLLLib;

using Ruler.Contracts.Services;
using Ruler.Contracts.Services.Verification;
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
    public class SecurityService : ISecurityService
    {
        private const int KeySizeBits = 2048;

        public IActiveKeyVerifier ActiveVerifier { get; }
        public IMasterFallbackVerifier MasterVerifier { get; }

        // Securely paste your production public key XML constants here
        private const string ActivePublicKeyXml = @"<RSAKeyValue><Modulus>...</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
        private const string MasterPublicKeyXml = @"<RSAKeyValue><Modulus>...</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        public SecurityService(IActiveKeyVerifier activeVerifier, IMasterFallbackVerifier masterVerifier)
        {
            ActiveVerifier = activeVerifier ?? throw new ArgumentNullException(nameof(activeVerifier));
            MasterVerifier = masterVerifier ?? throw new ArgumentNullException(nameof(masterVerifier));
        }

        public string GetActivePublicKeyXml() => ActivePublicKeyXml;
        public string GetMasterPublicKeyXml() => MasterPublicKeyXml;

        // --- KEEP YOUR CORE GENERATION & UTILITY ROUTINES ---

        public void GenerateAndSaveNewKeyPair(string environmentVariableName)
        {
            using (var rsa = new RSACryptoServiceProvider(KeySizeBits))
            {
                string privateKeyXml = rsa.ToXmlString(true);
                Environment.SetEnvironmentVariable(environmentVariableName, privateKeyXml, EnvironmentVariableTarget.User);
            }
        }

        public string LoadPrivateKeyFromEnvironment(string environmentVariableName)
        {
            string key = Environment.GetEnvironmentVariable(environmentVariableName, EnvironmentVariableTarget.User)
                         ?? Environment.GetEnvironmentVariable(environmentVariableName, EnvironmentVariableTarget.Machine);

            if (string.IsNullOrEmpty(key))
                throw new InvalidOperationException($"Cryptographic Private Key variable '{environmentVariableName}' could not be located.");

            return key;
        }

        public string SignData(string plainTextData, string privateKeyXml)
        {
            if (string.IsNullOrEmpty(plainTextData) || string.IsNullOrEmpty(privateKeyXml)) throw new ArgumentNullException();
            byte[] dataBytes = Encoding.UTF8.GetBytes(plainTextData);

            using (var rsa = new RSACryptoServiceProvider(KeySizeBits))
            {
                rsa.FromXmlString(privateKeyXml);
                byte[] signatureBytes = rsa.SignData(dataBytes, CryptoConfig.MapNameToOID("SHA256"));
                return Convert.ToBase64String(signatureBytes);
            }
        }

        public byte[] SignFile(string filePath, string privateKeyXml)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException("Target signature file not found", filePath);
            using (var rsa = new RSACryptoServiceProvider(KeySizeBits))
            {
                rsa.FromXmlString(privateKeyXml);
                using (var fileStream = File.OpenRead(filePath))
                {
                    return rsa.SignData(fileStream, CryptoConfig.MapNameToOID("SHA256"));
                }
            }
        }

        public X509Certificate2 GetMachineCertificate(string subjectName, StoreName storeName = StoreName.My, StoreLocation storeLocation = StoreLocation.LocalMachine)
        {
            var store = new X509Store(storeName, storeLocation);
            try
            {
                store.Open(OpenFlags.ReadOnly);
                var certCollection = store.Certificates.Find(X509FindType.FindBySubjectName, subjectName, validOnly: true);
                return certCollection.Count == 0 ? null : certCollection[0];
            }
            finally { store.Close(); }
        }

        public X509Certificate2 GenerateMachineCertificate(string subjectName, int validYears = 5, StoreName storeName = StoreName.My, StoreLocation storeLocation = StoreLocation.LocalMachine)
        {
            if (string.IsNullOrEmpty(subjectName)) throw new ArgumentNullException(nameof(subjectName));
            string fullSubjectDn = subjectName.StartsWith("CN=", StringComparison.OrdinalIgnoreCase) ? subjectName : $"CN={subjectName}";

            var privateKey = new CX509PrivateKey
            {
                ProviderName = "Microsoft Enhanced Cryptographic Provider v1.0",
                KeySpec = (X509KeySpec)2,
                Length = 2048,
                MachineContext = (storeLocation == StoreLocation.LocalMachine),
                ExportPolicy = X509PrivateKeyExportFlags.XCN_NCRYPT_ALLOW_EXPORT_NONE
            };
            privateKey.Create();

            var certRequest = new CX509CertificateRequestCertificate();
            certRequest.InitializeFromPrivateKey(
                (storeLocation == StoreLocation.LocalMachine) ? (X509CertificateEnrollmentContext)2 : (X509CertificateEnrollmentContext)1,
                privateKey,
                string.Empty
            );

            var x500Name = new CX500DistinguishedName();
            x500Name.Encode(fullSubjectDn, X500NameFlags.XCN_CERT_NAME_STR_NONE);
            certRequest.Subject = x500Name;
            certRequest.Issuer = certRequest.Subject;
            certRequest.NotBefore = DateTime.UtcNow.AddDays(-1);
            certRequest.NotAfter = DateTime.UtcNow.AddYears(validYears);

            var hashAlgorithm = new CObjectId();
            hashAlgorithm.InitializeFromValue("1.2.840.113549.1.1.11");
            certRequest.HashAlgorithm = hashAlgorithm;

            certRequest.Encode();
            var enrollment = new CX509Enrollment();
            enrollment.InitializeFromRequest(certRequest);

            string base64CertString = enrollment.CreatePFX(string.Empty, PFXExportOptions.PFXExportEEOnly);

            var certificate = new X509Certificate2(
                Convert.FromBase64String(base64CertString),
                string.Empty,
                X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet
            );

            var store = new X509Store(storeName, storeLocation);
            try
            {
                store.Open(OpenFlags.ReadWrite);
                store.Add(certificate);
            }
            finally { store.Close(); }

            return certificate;
        }
    }
}

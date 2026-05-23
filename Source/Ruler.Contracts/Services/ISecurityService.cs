using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using Ruler.Contracts.Services.Verification;

namespace Ruler.Contracts.Services
{
    public interface ISecurityService
    {
        // --- 1. DECOUPLED CRYPTOGRAPHIC VERIFIER ENGINES ---

        /// <summary>
        /// Gets the primary routine cryptographic engine for day-to-day active key checks.
        /// </summary>
        IActiveKeyVerifier ActiveVerifier { get; }

        /// <summary>
        /// Gets the emergency root cryptographic engine for direct master key fallback verification.
        /// </summary>
        IMasterFallbackVerifier MasterVerifier { get; }


        // --- 2. PUBLIC KEY ASSIGNMENT ACCESSORS ---

        /// <summary>
        /// Retrieves the current Active Public Key configuration string in XML format.
        /// </summary>
        string GetActivePublicKeyXml();

        /// <summary>
        /// Retrieves the immutable, root Master Public Key configuration string in XML format.
        /// </summary>
        string GetMasterPublicKeyXml();


        // --- 3. KEY PAIR LIFECYCLE MANAGEMENT ---

        /// <summary>
        /// Generates a secure RSA key pair and automatically writes the Private Key XML 
        /// into the specified system environment variable target.
        /// </summary>
        void GenerateAndSaveNewKeyPair(string environmentVariableName);

        /// <summary>
        /// Discovers and retrieves an RSA private key layout from the environment variables.
        /// </summary>
        string LoadPrivateKeyFromEnvironment(string environmentVariableName);


        // --- 4. SIGNING PAYLOAD OPERATIONS (USED BY GENERATOR UTILITIES) ---

        /// <summary>
        /// Computes a digital signature for plain-text metadata (like update.json) using an RSA private key.
        /// </summary>
        /// <returns>A Base64-encoded digital signature string.</returns>
        string SignData(string plainTextData, string privateKeyXml);

        /// <summary>
        /// Computes a digital signature for a local physical binary file (like an update ZIP or EXE).
        /// </summary>
        /// <returns>The raw cryptographic signature bytes.</returns>
        byte[] SignFile(string filePath, string privateKeyXml);


        // --- 5. WINDOWS CERTIFICATE STORE WORKFLOWS ---

        /// <summary>
        /// Locates an existing, valid application machine certificate from the local Windows certificate store ecosystem.
        /// </summary>
        X509Certificate2 GetMachineCertificate(
            string subjectName,
            StoreName storeName = StoreName.My,
            StoreLocation storeLocation = StoreLocation.LocalMachine);

        /// <summary>
        /// Programmatically creates a secure self-signed X509 machine certificate 
        /// and installs it directly into the targeted Windows Certificate Store.
        /// </summary>
        X509Certificate2 GenerateMachineCertificate(
            string subjectName,
            int validYears = 5,
            StoreName storeName = StoreName.My,
            StoreLocation storeLocation = StoreLocation.LocalMachine);
    }
}

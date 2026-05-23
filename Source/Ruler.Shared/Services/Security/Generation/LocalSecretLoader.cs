using Ruler.Contracts.Services.Generation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Services.Security.Generation
{
    public class LocalSecretLoader : ILocalSecretLoader
    {
        /// <summary>
        /// Attempts to locate and read a secret string from the environment.
        /// Checks the Current User scope first, then falls back to the Local Machine scope.
        /// </summary>
        public string GetSecret(string variableName)
        {
            if (string.IsNullOrWhiteSpace(variableName))
            {
                throw new ArgumentNullException(nameof(variableName), "Environment variable name cannot be null or empty.");
            }

            // 1. Primary Look-up: Look inside the Current User target environment
            string secret = Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.User);

            // 2. Fallback Look-up: If not found, look inside the Local Machine target environment
            if (string.IsNullOrEmpty(secret))
            {
                secret = Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.Machine);
            }

            // 3. Validation Guard: If both locations are empty, throw a descriptive, developer-friendly exception
            if (string.IsNullOrEmpty(secret))
            {
                throw new InvalidOperationException(
                    $"Cryptographic Secret Error: The environment variable '{variableName}' could not be located. " +
                    "Please ensure your environment variable path is configured or that you have run Ruler.Generator to initialize keys."
                );
            }

            return secret.Trim();
        }

        /// <summary>
        /// Fast verification tool to check if the workspace is configured without throwing exceptions.
        /// </summary>
        public bool SecretExists(string variableName)
        {
            if (string.IsNullOrWhiteSpace(variableName)) return false;

            string userKey = Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.User);
            if (!string.IsNullOrEmpty(userKey)) return true;

            string machineKey = Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.Machine);
            return !string.IsNullOrEmpty(machineKey);
        }
    }
}

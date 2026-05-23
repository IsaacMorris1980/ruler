using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Contracts.Services.Generation
{
    public interface ILocalSecretLoader
    {
        /// <summary>
        /// Retrieves a raw secret string from the configured environment variables.
        /// </summary>
        /// <param name="variableName">The target environment variable key name.</param>
        /// <returns>The raw secret string data (e.g., Private Key XML or connection string).</returns>
        string GetSecret(string variableName);

        /// <summary>
        /// Checks if the required cryptographic environment variable is present on the machine.
        /// </summary>
        bool SecretExists(string variableName);
    }
}

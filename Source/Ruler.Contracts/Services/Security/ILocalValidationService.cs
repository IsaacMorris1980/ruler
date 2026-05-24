using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Contracts.Services.Security
{
    public interface ILocalValidationService
    {
        /// <summary>
        /// Structurally parses a JSON string to ensure it is not structurally corrupted.
        /// </summary>
        bool IsConfigurationValid(string configJson);

        /// <summary>
        /// Signs local application settings data using a Windows Local Machine Certificate.
        /// </summary>
        string SignData(string data, string certificateSubjectName);

        /// <summary>
        /// Verifies local application settings data against its signature using a Windows Local Machine Certificate.
        /// </summary>
        bool VerifyData(string data, string base64Signature, string certificateSubjectName);
    }
}

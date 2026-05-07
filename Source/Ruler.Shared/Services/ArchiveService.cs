using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Services
{
    public class ArchiveService
    {
        /// <summary>
        /// Extracts a zip file to a designated temporary directory.
        /// </summary>
        /// <param name="zipPath">The path to the downloaded .zip file.</param>
        /// <returns>The path to the folder containing the extracted files.</returns>
        public string ExtractUpdate(string zipPath)
        {
            if (!File.Exists(zipPath))
                throw new FileNotFoundException("Update package not found.", zipPath);

            // Create a unique extraction folder inside Temp
            string extractionPath = Path.Combine(
                Path.GetDirectoryName(zipPath),
                "Extracted_" + DateTime.Now.Ticks
            );

            if (Directory.Exists(extractionPath))
                Directory.Delete(extractionPath, true);

            Directory.CreateDirectory(extractionPath);

            try
            {
                // Extract all contents
                ZipFile.ExtractToDirectory(zipPath, extractionPath);
                return extractionPath;
            }
            catch (Exception ex)
            {
                // Log extraction error (disk full, file locked, etc.)
                throw new Exception("Failed to extract update package.", ex);
            }
        }

        /// <summary>
        /// Deletes the temporary zip and extracted folders after the update process finishes.
        /// </summary>
        public void CleanUp(string zipPath, string extractionPath)
        {
            try
            {
                if (File.Exists(zipPath)) File.Delete(zipPath);
                if (Directory.Exists(extractionPath)) Directory.Delete(extractionPath, true);
            }
            catch
            {
                // We ignore cleanup errors; the OS or the next app launch will handle it.
            }
        }
    }
}

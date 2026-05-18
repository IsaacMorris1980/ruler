using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;   

namespace Ruler.Updator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3) return;

            string stagingPath = args[0];
            string destinationPath = args[1];
            int parentPid = int.Parse(args[2]);

            try
            {
                // 1. Wait for the main Ruler.exe process to actually exit
                var parentProcess = Process.GetProcessById(parentPid);
                parentProcess?.WaitForExit(10000); // Wait up to 10 seconds

                // 2. Copy verified files from staging to destination
                foreach (string newFilePath in Directory.GetFiles(stagingPath))
                {
                    string fileName = Path.GetFileName(newFilePath);
                    string destFile = Path.Combine(destinationPath, fileName);

                    // Overwrite the old binaries
                    File.Copy(newFilePath, destFile, true);
                }

                // 3. Clean up the staging folder
                Directory.Delete(stagingPath, true);

                // 4. Restart the newly updated Ruler.exe
                Process.Start(Path.Combine(destinationPath, "Ruler.exe"));
            }
            catch (Exception ex)
            {
                // If something fails, we want to know why
                File.WriteAllText("update_error.log", ex.ToString());
            }
        }
    }
}

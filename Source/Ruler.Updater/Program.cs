using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Updater
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // args[0] = Source (Extracted Temp Folder)
            // args[1] = Target (App Installation Folder)
            // args[2] = PID

            if (args.Length < 3) return;

            string sourceDir = args[0];
            string targetDir = args[1];
            int mainAppId = int.Parse(args[2]);

            // 1. Wait for MainApp to release file locks
            try
            {
                var p = Process.GetProcessById(mainAppId);
                p.WaitForExit(5000);
            }
            catch { /* Already closed */ }

            try
            {
                // 2. Define exactly what we want to move
                // We use a string array to specify the exact filenames
                string[] filesToUpdate = { "MainApp.exe", "Updater.exe" };

                foreach (string fileName in filesToUpdate)
                {
                    string sourceFile = Path.Combine(sourceDir, fileName);
                    string targetFile = Path.Combine(targetDir, fileName);

                    if (File.Exists(sourceFile))
                    {
                        // Move the file (Overwrite if exists)
                        File.Copy(sourceFile, targetFile, true);
                        Console.WriteLine($"Successfully updated {fileName}");
                    }
                }

                // 3. Relaunch the fresh MainApp
                Process.Start(Path.Combine(targetDir, "MainApp.exe"));
            }
            catch (Exception ex)
            {
                // Log error to a local file since the console will close
                File.WriteAllText(Path.Combine(targetDir, "update_log.txt"), ex.ToString());
            }
        }
    }
}

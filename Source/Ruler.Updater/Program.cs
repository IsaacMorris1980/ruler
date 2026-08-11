using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ruler.Updater
{
    class Program
    {
        static void Main(string[] args)
        {
            // Expected 3 arguments:
            // args[0] = Path to Ruler.exe (to restart)
            // args[1] = Source path (staging directory containing the new verified executable)
            // args[2] = Target path (main installation directory)
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: Ruler.Updater <rulerExePath> <sourceDirectory> <targetDirectory>");
                return;
            }

            string rulerExePath = args[0];
            string sourceDirectory = args[1];
            string targetDirectory = args[2];

            Console.WriteLine("Ruler Updater initiated...");

            try
            {
                // 1. Give the main application time to completely shut down and release file locks
                Thread.Sleep(1500);

                // Explicitly wait for any lingering Ruler process to exit
                try
                {
                    foreach (var process in Process.GetProcessesByName("Ruler"))
                    {
                        process.WaitForExit(5000);
                    }
                }
                catch { }

                // 2. Validate source directory exists
                if (!Directory.Exists(sourceDirectory))
                {
                    Console.WriteLine("Error: Source staging directory does not exist.");
                    return;
                }

                // 3. Copy only the executable file
                string exeName = Path.GetFileName(rulerExePath); // e.g., "Ruler.exe"
                string sourceFile = Path.Combine(sourceDirectory, exeName);
                string targetFile = Path.Combine(targetDirectory, exeName);

                if (File.Exists(sourceFile))
                {
                    Console.WriteLine($"Copying updated executable to: {targetFile}");
                    File.Copy(sourceFile, targetFile, overwrite: true);
                }
                else
                {
                    Console.WriteLine($"Error: Executable '{exeName}' not found in source staging directory.");
                    return;
                }

                // 4. Comprehensive Cleanup Routine
                Console.WriteLine("Cleaning up temporary update files and archives...");

                // Delete staging folder with a retry loop in case files are briefly locked
                DeleteDirectoryWithRetry(sourceDirectory);

                // Purge any leftover temporary update ZIP files in the system temp folder
                PurgeTempUpdateZips();

                // 5. Restart the updated Ruler application
                if (File.Exists(rulerExePath))
                {
                    Console.WriteLine("Restarting Ruler...");
                    Process.Start(rulerExePath);
                }
                else
                {
                    Console.WriteLine($"Warning: Executable not found at {rulerExePath}, unable to restart automatically.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Critical error during update execution: {ex.Message}");
            }
        }

        private static void DeleteDirectoryWithRetry(string path, int maxRetries = 3, int delayMs = 500)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    if (Directory.Exists(path))
                    {
                        Directory.Delete(path, recursive: true);
                        break;
                    }
                }
                catch
                {
                    if (i == maxRetries - 1)
                    {
                        throw;
                    }
                    Thread.Sleep(delayMs);
                }
            }
        }

        private static void PurgeTempUpdateZips()
        {
            try
            {
                string tempPath = Path.GetTempPath();
                string[] leftoverZips = Directory.GetFiles(tempPath, "RulerUpdate_*.zip");
                foreach (string zip in leftoverZips)
                {
                    try
                    {
                        File.Delete(zip);
                    }
                    catch
                    {
                        // Suppress individual file deletion locks if open elsewhere 
                    }
                }
            }
            catch
            {
                // Suppress errors during global temp scan
            }
        }
    }

}

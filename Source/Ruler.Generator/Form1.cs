using Microsoft.VisualBasic;

using Newtonsoft.Json;

using Ruler.Shared.Models;
using Ruler.Shared.Services;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ruler.Generator
{
    public partial class Form1 : Form
    {
        private SecurityService security = new SecurityService();
        public Form1()
        {
            InitializeComponent();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ziptxt.Text) || !File.Exists(ziptxt.Text))
            {
                MessageBox.Show("Please select a valid Zip file.");
                return;
            }
            button8.Enabled = true; // Enable Generate Signed manifest after generating hash
        }
        public string CalculateFileHash(string filePath)
        {
            if (!File.Exists(filePath)) return string.Empty;

            using (var sha256 = SHA256.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    byte[] hashBytes = sha256.ComputeHash(stream);

                    // Convert bytes to a readable hex string
                    StringBuilder sb = new StringBuilder();
                    foreach (byte b in hashBytes)
                    {
                        sb.Append(b.ToString("x2"));
                    }
                    return sb.ToString();
                }
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Personal Information Exchange (*.pfx;*.p12)|*.pfx;*.p12|All files (*.*)|*.*";
            openFileDialog.Title = "Select your Code Signing Certificate";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.CheckFileExists = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                pfxtxt.Text = openFileDialog.FileName;
            }
           
        }

        private void button5_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Executable Files (*.exe)|*.exe|All files (*.*)|*.*";
            openFileDialog.Title = "Select your Ruler Executable File";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.CheckFileExists = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                rulertxt.Text = openFileDialog.FileName;
            }
           
        }

        private void button6_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Executable Files (*.exe)|*.exe|All files (*.*)|*.*";
            openFileDialog.Title = "Select your Updater Executable File";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.CheckFileExists = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                updatertxt.Text = openFileDialog.FileName;
            }
          
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Zip Files (*.zip)|*.zip|All files (*.*)|*.*";
            openFileDialog.Title = "Select your Zip File";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.CheckFileExists = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                ziptxt.Text = openFileDialog.FileName;
            }
          
        }
        private bool SignExecutable(string exePath, string pfxPath, string pfxPassword)
        {
            // Path to signtool.exe (ensure this matches your Windows SDK version)
            string signtool = @"C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64\signtool.exe";

            // /f = path to PFX
            // /p = THE PASSWORD YOU CREATED THE CERT WITH
            // /t = Timestamp server (so signature stays valid after cert expires)
            string args = $"/sign /f \"{pfxPath}\" /p \"{pfxPassword}\" /t http://timestamp.digicert.com /v \"{exePath}\"";

            var startInfo = new ProcessStartInfo
            {
                FileName = signtool,
                Arguments = args,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (var process = Process.Start(startInfo))
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new Exception($"Signing failed: {error}");
                }
                return true;
            }
        }
        private void button9_Click(object sender, EventArgs e)
        {
            string password = Interaction.InputBox("Enter self signed certificate password", "Certificate Password", "", -1, -1);
            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter a password to protect the PFX file.");
                return;
            }

            using (SaveFileDialog saveFile = new SaveFileDialog())
            {
                saveFile.Filter = "PFX Files (*.pfx)|*.pfx";
                saveFile.FileName = "RulerSecurity.pfx";

                if (saveFile.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // 1. Create an RSA key for the certificate
                        using (RSA rsa = RSA.Create(2048))
                        {
                            // 2. Create the Certificate Request
                            // "CN=RulerApp" identifies the "Issuer"
                            var request = new CertificateRequest(
                                "CN=RulerApp-Developer",
                                rsa,
                                HashAlgorithmName.SHA256,
                                RSASignaturePadding.Pkcs1);

                            // 3. Add Code Signing purpose (OID 1.3.6.1.5.5.7.3.3)
                            request.CertificateExtensions.Add(
                                new X509EnhancedKeyUsageExtension(
                                    new OidCollection { new Oid("1.3.6.1.5.5.7.3.3") },
                                    false));

                            // 4. Create the self-signed cert (Valid for 3 years)
                            using (X509Certificate2 cert = request.CreateSelfSigned(
                                DateTimeOffset.Now,
                                DateTimeOffset.Now.AddYears(3)))
                            {
                                // 5. Export as PFX with the password
                                byte[] pfxBytes = cert.Export(X509ContentType.Pfx, password);
                                File.WriteAllBytes(saveFile.FileName, pfxBytes);

                                // 6. Show the thumbprint so you can add it to SecurityService
                                string thumbprint = cert.Thumbprint;
                               pfxtxt.Text = saveFile.FileName; // Auto-load the new file

                                MessageBox.Show($"PFX Created!\n\nThumbprint: {thumbprint}\n\nCopy this thumbprint to your SecurityService.cs list.", "Success");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error creating certificate: {ex.Message}");
                    }
                }
            }


        }
        private void SaveKeyToFile(string keyContent, string defaultName, string title)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
                sfd.FileName = defaultName;
                sfd.Title = title;

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(sfd.FileName, keyContent);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                // True = Export Private + Public
                string privateKeyXml = rsa.ToXmlString(true);

                // False = Export ONLY Public
                string publicKeyXml = rsa.ToXmlString(false);

                // 1. Save Private Key (The Secret "Voice")
                SaveKeyToFile(privateKeyXml, "Ruler_PRIVATE_Key.xml", "Save Private Key (KEEP SECRET)");

                // 2. Save Public Key (The "Memory" for your App)
                SaveKeyToFile(publicKeyXml, "Ruler_PUBLIC_Key.xml", "Save Public Key (For Shared Project)");

                MessageBox.Show("Keys generated! Now copy the content of the Public Key file into your SecurityService.cs constant.", "Success");
            }
        }

        private void certpwtxt_TextChanged(object sender, EventArgs e)
        {
            button8.Enabled = !string.IsNullOrWhiteSpace(certpwtxt.Text) && !string.IsNullOrWhiteSpace(pfxtxt.Text);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            pbStatus.Value = 0;
            lblStep.Text = "Starting build...";
            Application.DoEvents(); // Force UI to refresh in .NET 4.8
            Thread.Sleep(1000); // Simulate some initial delay
            lblStep.Text = "Validating inputs...";
            if (!IsInputValid())
            {
                return;
            }
          UpdateProgress(1);
            // 1. Get the folder the file is in (bin\Release)
            string currentFolder = Path.GetDirectoryName(rulertxt.Text);

            // 2. Get the folder above that (bin)
            string parentFolder = Directory.GetParent(currentFolder).FullName;

            string zipOutput = Path.Combine(parentFolder, $"Ruler_v{vertxt.Text}_{DateTime.Now.ToString("mm-dd-yyyy")}.zip");
            //SignExecutable(rulertxt.Text, pfxtxt.Text, certpwtxt.Text);
            //SignExecutable(updatertxt.Text, pfxtxt.Text, certpwtxt.Text);

            //try
            //{
            //    string buildFolder = currentFolder;
            //    string zipOutput = Path.Combine(parentFolder, $"Ruler_v{vertxt.Text}.zip");

            //    // 1. Zip the files
            //    CreatePackage(buildFolder, zipOutput);

            //    // 2. Hash the newly created zip
            //    string hash = security.CalculateFileHash(zipOutput);


            //    MessageBox.Show("Release ZIP created and hashed!");
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"Zipping failed: {ex.Message}");
            //}

            // Initialize UI
           

            try
            {
                // --- STAGE 1: CLEANUP ---
                lblStep.Text = "Stage 1/6: Cleaning and Archiving old releases...";
                if (!CleanupAndArchive(parentFolder)) return;
                UpdateProgress(2);

                // --- STAGE 2: SIGNING ---
                lblStep.Text = "Stage 2/6: Digitally signing executables...";
                string[] exes = { "Ruler.exe", "Updater.exe" };
                foreach (string exe in exes)
                {
                    string path = Path.Combine(currentFolder, exe);
                    if (!SignFile(path, pfxtxt.Text, certpwtxt.Text)) return;
                }
                UpdateProgress(3);

                // --- STAGE 3: VERIFICATION ---
                lblStep.Text = "Stage 3/6: Verifying digital signatures...";
                foreach (string exe in exes)
                {
                    if (!VerifySignature(Path.Combine(currentFolder, exe))) return;
                }
                UpdateProgress(4);

                // --- STAGE 4: ZIPPING ---
                lblStep.Text = "Stage 4/6: Creating ZIP package...";
                string zipPath = Path.Combine(parentFolder, $"Ruler_v{vertxt.Text}_{DateTime.Now.ToString("MM-dd-yyyy")}.zip");
                if (!CreateZip(currentFolder, zipPath)) return;
                UpdateProgress(5);

                // --- STAGE 5: MANIFEST ---
                lblStep.Text = "Stage 5/6: Calculating hashes and building manifest...";
                var manifest = BuildManifestObject(currentFolder, zipPath, vertxt.Text);
                if (manifest == null) return;
                UpdateProgress(6);

                // --- STAGE 6: RSA SIGNING ---
                lblStep.Text = "Stage 6/6: Finalizing RSA manifest signature...";
                if (!SaveAndSignManifest(manifest,parentFolder)) return;
                UpdateProgress(7);

                lblStep.Text = "Build Complete!";
                MessageBox.Show("All stages passed! Release is ready for deployment.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                lblStep.Text = "Build Failed.";
                MessageBox.Show($"Critical Error: {ex.Message}", "Pipeline Aborted", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateProgress(int step)
        {
            pbStatus.Value = step;
            Application.DoEvents(); // Keeps the WinForm responsive during the "Massive Method"
        }

        
        private void CreatePackage(string sourceFolderPath, string destinationZipPath)
        {
            // If the old zip exists, delete it first or CreateFromDirectory will throw an error
            if (File.Exists(destinationZipPath))
            {
                File.Delete(destinationZipPath);
            }

            // This zips EVERYTHING in the folder
            ZipFile.CreateFromDirectory(sourceFolderPath, destinationZipPath, CompressionLevel.Optimal, false);
        }
        private bool CleanupAndArchive(string releasePath)
        {
            try
            {
                string archiveDir = Path.Combine(releasePath, "_Archive");
                if (!Directory.Exists(archiveDir)) Directory.CreateDirectory(archiveDir);

                string stamp = DateTime.Now.ToString("MM-dd-yyyy");
                foreach (string oldZip in Directory.GetFiles(releasePath, "*.zip"))
                {
                    string movePath = Path.Combine(archiveDir, Path.GetFileNameWithoutExtension(oldZip) + "_" + stamp + ".zip");
                    if (File.Exists(movePath)) File.Delete(movePath);
                    File.Move(oldZip, movePath);
                }

                string manifestPath = Path.Combine(releasePath, "manifest.json");
                if (File.Exists(manifestPath)) File.Delete(manifestPath);
                if (File.Exists(manifestPath + ".sig")) File.Delete(manifestPath + ".sig");

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Archive Error: " + ex.Message);
                return false;
            }
        }
        private bool CreateZip(string source, string dest)
        {
            try
            {
                if (File.Exists(dest)) File.Delete(dest);
                ZipFile.CreateFromDirectory(source, dest);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Zip Error: " + ex.Message);
                return false;
            }
        }
        private bool IsInputValid()
        {
            // 1. Check File Paths
            if (!File.Exists(pfxtxt.Text))
                return ValidationError("Certificate file not found.");

            if (!File.Exists(rulertxt.Text))
                return ValidationError("Ruler App executable not found.");

            if (!File.Exists(updatertxt.Text))
                return ValidationError("Updater App executable not found.");

            // 2. Check Password
            if (string.IsNullOrWhiteSpace(certpwtxt.Text))
                return ValidationError("Please enter the PFX password.");

            // 3. Check Version (Regex for format 1.0.0)
            if (!System.Text.RegularExpressions.Regex.IsMatch(vertxt.Text, @"^\d+\.\d+\.\d+\.\d+$"))
                return ValidationError("Version must be in format '1.0.0.0'.");

            return true; // Everything is good!
        }

        // Helper to show message and return false
        private bool ValidationError(string message)
        {
            MessageBox.Show(message, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        private bool SignFile(string path, string pfx, string pass)
        {
            int retries = 3;
            while (retries > 0)
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "signtool.exe",
                    Arguments = $"sign /f \"{pfx}\" /p {pass} /t http://timestamp.digicert.com \"{path}\"",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                };
                using (var proc = Process.Start(psi))
                {
                    proc.WaitForExit();
                    if (proc.ExitCode == 0) return true;
                }
                retries--;
                System.Threading.Thread.Sleep(2000);
            }
            return false;
        }
        private bool VerifySignature(string path)
        {
            try
            {
                var cert = System.Security.Cryptography.X509Certificates.X509Certificate.CreateFromSignedFile(path);
                return cert != null;
            }
            catch { return false; }
        }
        private bool SaveAndSignManifest(UpdateManifest manifest, string releaseDir)
        {
            try
            {
                string json = JsonConvert.SerializeObject(manifest, Formatting.Indented);
                string sig = security.SignData(json, pfxtxt.Text, certpwtxt.Text); // Calls your RSA Private Key method

                File.WriteAllText(Path.Combine(releaseDir, "manifest.json"), json);
                File.WriteAllText(Path.Combine(releaseDir, "manifest.json.sig"), sig);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Manifest Error: " + ex.Message);
                return false;
            }
        }
        private UpdateManifest BuildManifestObject(string buildDir, string zipPath, string version)
        {
            var manifest = new UpdateManifest
            {
                Version = version,
                ReleaseDate = DateTime.Now.ToString("MM-dd-yyyy"),
                PackageHash = security.CalculateFileHash(zipPath),
                Files = new List<FileEntry>()
            };

            foreach (string exe in new[] { "Ruler.exe", "Updater.exe" })
            {
                string path = Path.Combine(buildDir, exe);
                manifest.Files.Add(new FileEntry
                {
                    FileName = exe,
                    Hash = security.CalculateFileHash(path),
                    Version = FileVersionInfo.GetVersionInfo(path).FileVersion,
                    Thumbprint = security.GetFileSignatureThumbprint(path)
                });
            }
            return manifest;
        }
    }
}

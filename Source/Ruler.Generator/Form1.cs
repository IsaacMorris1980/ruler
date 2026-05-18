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
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Ruler.Generator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private string pfxPath;
        private string rulerexePath;
        private string unpdateexePath;
        private Version rulerVersion;
        private Version updaterVersion;
        private void button6_Click(object sender, EventArgs e)
        {
            using (var pfxDialog = new OpenFileDialog())
            {
                pfxDialog.Filter = "PFX files (*.pfx)|*.pfx|All files (*.*)|*.*";
                pfxDialog.Title = "Select the PFX Certificate";
                if (pfxDialog.ShowDialog() == DialogResult.OK)
                {
                    textBoxPfxPath.Text = pfxDialog.FileName;
                    pfxPath = pfxDialog.FileName;
                }
            }
            using (var exeDialogn = new OpenFileDialog())
            {
                exeDialogn.Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*";
                exeDialogn.Title = "Select the Ruler Executable";
                if (exeDialogn.ShowDialog() == DialogResult.OK)
                {
                    textBoxExePath.Text = exeDialogn.FileName;
                    rulerexePath = exeDialogn.FileName;
                    rulerVersion = new Version(GetAssemblyVersion(rulerexePath));
                   textEXEVersion.Text = GetAssemblyVersion(rulerexePath);

                }
            }
            using (var exeDialog = new OpenFileDialog())
            {
                exeDialog.Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*";
                exeDialog.Title = "Select the Updater Executable";
                if (exeDialog.ShowDialog() == DialogResult.OK)
                {
                    textBoxUpdateExePath.Text = exeDialog.FileName;
                    unpdateexePath = exeDialog.FileName;
                    updaterVersion = new Version(GetAssemblyVersion(unpdateexePath));
                    textboxUpdaterVersion.Text = GetAssemblyVersion(unpdateexePath);

                }
            }
                
        }
        public string GetAssemblyVersion(string filePath)
        {
            try
            {
                // This loads only the metadata, not the whole library
                AssemblyName name = AssemblyName.GetAssemblyName(filePath);
                return name.Version.ToString();
            }
            catch
            {
                return "0.0.0.0";
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
           
            string version = FileVersionInfo.GetVersionInfo(rulerexePath).FileVersion;
            string sourceBinPath = Path.GetDirectoryName(rulerexePath);
            string outputDir = Directory.GetParent(sourceBinPath).FullName; // Output to parent of bin
            string zipFileName = $"Ruler_{version}.zip";
            string zipPath = Path.Combine(outputDir, zipFileName);
            string manifestPath = Path.Combine(outputDir, "update.json");
            string manifestSigPath = Path.Combine(outputDir, "update.json.sig"); // NEW

            // 2. Create the ZIP archive
            if (File.Exists(zipPath)) File.Delete(zipPath);
            using (ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                // Add the first file (Ruler EXE)
                if (File.Exists(textBoxExePath.Text))
                {
                    string entryName = Path.GetFileName(textBoxExePath.Text);
                    archive.CreateEntryFromFile(textBoxExePath.Text, entryName);
                }

                // Add the second file (Update EXE)
                if (File.Exists(textBoxUpdateExePath.Text))
                {
                    string entryName = Path.GetFileName(textBoxUpdateExePath.Text);
                    archive.CreateEntryFromFile(textBoxUpdateExePath.Text, entryName);
                }

                // You can keep adding more files here (like your Shared.dll)
            }

            // 3. Start building the Manifest
            var manifest = new UpdateManifest()
            {
                ReleaseVersion = version,
                ZipSignature = SecurityService.SignFile(zipPath),
                Files = new List<FileSignature>()
            };

            // 4. Hash and Sign individual files
          
                if (File.Exists(textBoxExePath.Text))
                {
                    manifest.Files.Add(new FileSignature
                    {
                        FileName = Path.GetFileName(textBoxExePath.Text),
                        Version = version,
                        Hash = SecurityService.GenerateHash(textBoxExePath.Text),
                        Signature = SecurityService.SignFile(textBoxExePath.Text)
                    });
                }
                if (File.Exists(textBoxUpdateExePath.Text))
                {
                    manifest.Files.Add(new FileSignature
                    {
                        FileName = Path.GetFileName(textBoxUpdateExePath.Text),
                        Version = version,
                        Hash = SecurityService.GenerateHash(textBoxUpdateExePath.Text),
                        Signature = SecurityService.SignFile(textBoxUpdateExePath.Text)
                    });
            }


            // 5. Serialize the Manifest to JSON
            string json = JsonConvert.SerializeObject(manifest, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(manifestPath, json);

            // 6. NEW: Sign the JSON string itself and save to .sig file
            // Use your Active Private Key for this step
            string manifestSignature = SecurityService.SignData(json);
            File.WriteAllText(manifestSigPath, manifestSignature);

            Console.WriteLine("Release packaged: ZIP, Manifest, and Manifest Signature created.");
        }
        private void button5_Click(object sender, EventArgs e)
        {
            string vaultDir = Environment.GetEnvironmentVariable("RULER_SECRETS_FOLDER");

            if (!Directory.Exists(vaultDir)) Directory.CreateDirectory(vaultDir);
            string activePrivatePath = Path.Combine(vaultDir, "Active_Private.xml");
            string activePublicPath = Path.Combine(vaultDir, "Active_Public.xml");      

            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                string activePrivate = rsa.ToXmlString(true);
                string activePublic = rsa.ToXmlString(false);
                Clipboard.SetText(rsa.ToXmlString(true));
                File.WriteAllText(activePrivatePath, activePrivate);
                File.SetAttributes(activePrivatePath, FileAttributes.Normal);
                File.WriteAllText(activePublicPath, activePublic);
                File.SetAttributes(activePublicPath, FileAttributes.Normal);
                //CopyPublicKeyToClipboard(activePublicPath, "Active");
                //System.Diagnostics.Process.Start("explorer.exe", "/select,\"" + activePrivatePath + "\"");
            }

            Console.WriteLine($" Active Keys successfully saved to: {vaultDir}");
            Console.WriteLine("CRITICAL: Keep 'Private' files secret. Copy 'Public' strings to Ruler.Shared.");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string vaultDir = Environment.GetEnvironmentVariable("RULER_SECRETS_FOLDER");

            if (!Directory.Exists(vaultDir)) Directory.CreateDirectory(vaultDir);
           
            string masterPrivatePath =$@"{vaultDir}\Master_Private.xml";
            string masterPublicPath = Path.Combine(vaultDir, "Master_Public.xml");

            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                // --- 1. MASTER KEY PAIR ---
                // Save these to a "Vault" folder that is NOT in your project directory
                string masterPrivate = rsa.ToXmlString(true);
                string masterPublic = rsa.ToXmlString(false);
                Clipboard.SetText(rsa.ToXmlString(false));
                File.WriteAllText(masterPrivatePath, masterPrivate);
                File.SetAttributes(masterPrivatePath, FileAttributes.Normal);
                File.WriteAllText(masterPublicPath, masterPublic);
                File.SetAttributes(masterPublicPath, FileAttributes.Normal);
                //System.Diagnostics.Process.Start("explorer.exe", "/select,\"" + masterPrivatePath + "\"");
                //CopyPublicKeyToClipboard(masterPublicPath, "Master");

            }
            Console.WriteLine($"Master Keys successfully saved to: {vaultDir}");
            Console.WriteLine("CRITICAL: Keep 'Private' files secret. Copy 'Public' strings to Ruler.Shared.");

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 1. Get the path from your environment variable
            // This explicitly ignores System/Machine variables and looks only at your User account
            string vaultDir = Environment.GetEnvironmentVariable("RULER_SECRETS_FOLDER", EnvironmentVariableTarget.User);

            if (string.IsNullOrEmpty(vaultDir))
            {
                // Fallback: If it's not in User, maybe you accidentally set it in Machine?
                vaultDir = Environment.GetEnvironmentVariable("RULER_SECRETS_FOLDER", EnvironmentVariableTarget.Machine);
            }

            if (string.IsNullOrEmpty(vaultDir))
            {
                MessageBox.Show("Could not find 'RULER_SECRETS_FOLDER' in User or System variables.", "Error");
                return;
            }

         string masterPath = Path.Combine(vaultDir, "Master_Private.xml");  

            // 2. Check if the Master Key already exists
            if (File.Exists(masterPath))
            {
                //if (File.Exists(masterPath))
                //{
                //    // This will open File Explorer and highlight the file the code is seeing
                //    System.Diagnostics.Process.Start("explorer.exe", "/select,\"" + masterPath + "\"");
                //}
                // The Master Key exists! Lock the door and hide the button.
                //button1.Enabled = false;
                //button1.Visible = false;
            }
        }
        public void CopyPublicKeyToClipboard(string publicPath, string masterOrActive)
        {
            try
            {
                // Use your verified path
              

                if (File.Exists(publicPath))
                {
                    string xmlContent = File.ReadAllText(publicPath);

                    // Set the text to clipboard
                    Clipboard.SetText(xmlContent);

                    MessageBox.Show($"{masterOrActive} public Key copied to clipboard for SecurityService!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"{masterOrActive} public Key file not found at " + publicPath, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to copy: " + ex.Message);
            }
        }
    }
    
}

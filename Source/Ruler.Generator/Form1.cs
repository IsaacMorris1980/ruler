using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ruler
{
    public partial class SetZoomLevelForm : Form
    {
        public SetZoomLevelForm(int ZoomLevel)
        {
            InitializeComponent();
            txtZoomLevel.Text = ZoomLevel.ToString();
            txtZoomLevel.GotFocus += TxtZoomLevel_GotFocus;
        }

        private void TxtZoomLevel_GotFocus(object sender, EventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        public int GetZoomSize()
        {
            return int.TryParse(txtZoomLevel.Text, out int zoomLevel) ? zoomLevel : 100;
        }
    }
}

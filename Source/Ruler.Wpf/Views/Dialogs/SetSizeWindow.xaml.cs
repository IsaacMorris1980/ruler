using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Ruler.Wpf.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for SetSizeWindow.xaml
    /// </summary>
    public partial class SetSizeWindow : Window
    {
        public SetSizeWindow()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // Close the frame and return 'true' to indicate data submission
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Abort modifications and return 'false' or 'null' back to caller
            this.DialogResult = false;
        }
    }
}

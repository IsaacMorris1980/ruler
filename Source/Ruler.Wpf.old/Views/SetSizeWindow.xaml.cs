using Ruler.Wpf.ViewModels;

using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace Ruler.Wpf.Views
{
    /// <summary>
    /// Interaction logic for SetSizeWindow.xaml
    /// </summary>
    public partial class SetSizeWindow : Window
    {
        public SetSizeWindow(SetSizeViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            // Listen for the ViewModel signaling that it's done processing
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SetSizeViewModel.DialogResult))
            {
                if (DataContext is SetSizeViewModel vm && vm.DialogResult.HasValue)
                {
                    // Setting this automatically closes the modal dialog 
                    // and returns the value to whoever called .ShowDialog()
                    this.DialogResult = vm.DialogResult.Value;
                }
            }
        }
    }
}

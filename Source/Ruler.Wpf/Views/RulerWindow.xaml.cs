using Ruler.Wpf.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace Ruler.Wpf.Views
{
    /// <summary>
    /// Interaction logic for RulerWindow.xaml
    /// </summary>
    public partial class RulerWindow : Window
    {
        public RulerWindow()
        {
            InitializeComponent();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is RulerViewModel vm && vm.IsLocked)
                return; // Guard against moving if locked

            // Native WPF drag logic replaces standard Win32 handle offsets
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}

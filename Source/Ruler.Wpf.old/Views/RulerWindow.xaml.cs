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

        /// <summary>
        /// Allows the border surface to act as a drag handler anywhere on screen.
        /// </summary>
        private void RulerWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove(); // Native WPF handler for moving borderless windows
            }
        }

        /// <summary>
        /// Direct link to terminate this specific window instance from the Context Menu.
        /// </summary>
        private void CloseMenu_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {

        }

        private void Window_ContextMenuClosing(object sender, ContextMenuEventArgs e)
        {

        }
    }
}

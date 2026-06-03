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
using Ruler.Wpf.Models;

namespace Ruler.Wpf.Views
{
    /// <summary>
    /// Interaction logic for InformationalWindow.xaml
    /// </summary>
    public partial class InformationalWindow : Window
    {
        public InformationalWindow()
        {
            InitializeComponent();
            PopulateShortcutLists();
        }

        private void PopulateShortcutLists()
        {
            // Grab the template resource we declared in the XAML
            var rowTemplate = (DataTemplate)this.FindResource("ShortcutRowTemplate");

            // Assign templates to layout buckets
            NavigationItems.ItemTemplate = rowTemplate;
            ActionItems.ItemTemplate = rowTemplate;
            ResetItems.ItemTemplate = rowTemplate;

            // Group 1: Navigation & Sizing
            NavigationItems.ItemsSource = new List<ShortcutItem>
            {
                new ShortcutItem { Keys = "Arrows", Description = "Move window position (5px steps)" },
                new ShortcutItem { Keys = "Shift + Arrows", Description = "Micro-move window position (1px fine-tuning)" },
                new ShortcutItem { Keys = "Ctrl + Arrows", Description = "Resize window dimensions (5px steps)" },
                new ShortcutItem { Keys = "Ctrl + Shift + Arrows", Description = "Micro-resize window dimensions (1px fine-tuning)" }
            };

            // Group 2: Actions
            ActionItems.ItemsSource = new List<ShortcutItem>
            {
                new ShortcutItem { Keys = "Space", Description = "Toggle Layout Orientation (Horizontal / Vertical)" },
                new ShortcutItem { Keys = "Ctrl + S", Description = "Open manual 'Set Size' adjustment panel" },
                new ShortcutItem { Keys = "Ctrl + U", Description = "Check online repository for application updates" },
                new ShortcutItem { Keys = "+ / -", Description = "Nudge ruler layout opacity up / down by 10%" },
                new ShortcutItem { Keys = "Ctrl + +", Description = "Snap ruler opacity instantly to 100% maximum" },
                new ShortcutItem { Keys = "Ctrl + -", Description = "Snap ruler opacity instantly to 20% minimum" }
            };

            // Group 3: Resets & App Closures
            ResetItems.ItemsSource = new List<ShortcutItem>
            {
                new ShortcutItem { Keys = "R", Description = "Reset active ruler window layout parameters" },
                new ShortcutItem { Keys = "Ctrl + R", Description = "Reset ALL rulers in the session list (including closed)" },
                new ShortcutItem { Keys = "I", Description = "Open this Shortcuts Reference Guide panel" },
                new ShortcutItem { Keys = "Escape", Description = "Close down the currently selected ruler instance" }
            };
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

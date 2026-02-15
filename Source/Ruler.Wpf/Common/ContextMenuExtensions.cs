using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Ruler.Wpf.Common
{
    public static class ContextMenuExtensions
    {
        /// <summary>
        /// Displays the ContextMenu at specific screen coordinates.
        /// </summary>
        /// <param name="menu">The ContextMenu to show.</param>
        /// <param name="x">The X coordinate (DIPs).</param>
        /// <param name="y">The Y coordinate (DIPs).</param>
        /// <param name="placementTarget">The visual element that "owns" this menu (used for styling and focus).</param>
        public static void ShowAt(this ContextMenu menu, double x, double y, UIElement placementTarget)
        {
            if (menu == null) return;

            // Set the placement target so the menu inherits DataContext and Styles
            menu.PlacementTarget = placementTarget;

            // PlacementMode.Absolute allows us to use Horizontal/VerticalOffset as screen coordinates
            menu.Placement = PlacementMode.Absolute;

            menu.HorizontalOffset = x;
            menu.VerticalOffset = y;

            // Force the menu to open
            menu.IsOpen = true;
        }
    }
}

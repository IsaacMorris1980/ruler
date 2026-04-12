using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared
{
    /// <summary>
    /// Defines the specific part of the ruler the mouse is interacting with.
    /// Used to determine cursor type and calculation logic during a drag.
    /// </summary>
    public enum ResizeEdge
    {
        /// <summary> No edge detected (Default/Move state) </summary>
        None,
        // --- Corners (Priority for Hit-Testing) ---
        /// <summary> The Top-Left origin point (0,0 in Ruler Space) </summary>
        TopLeft,
        /// <summary> The Top-Right corner (Width, 0) </summary>
        TopRight,
        /// <summary> The Bottom-Left corner (0, Height) </summary>
        BottomLeft,
        /// <summary> The Bottom-Right corner (Width, Height) </summary>
        BottomRight,
        // --- Edges ---
        /// <summary> The horizontal top boundary line </summary>
        Top,
        /// <summary> The horizontal bottom boundary line </summary>
        Bottom,
        /// <summary> The vertical start boundary (The '0' end) </summary>
        Left,
        /// <summary> The vertical end boundary (The far end) </summary>
        Right
    }
}

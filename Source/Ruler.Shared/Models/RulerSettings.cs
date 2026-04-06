using System;

namespace Ruler.Shared
{
    [Serializable]
    public class RulerSettings
    {
        // --- Identity & Persistence ---
        /// <summary>
        /// Unique ID for this specific ruler instance. 
        /// Used to track multiple rulers in the settings file.
        /// </summary>
        public Guid InstanceId { get; set; } = Guid.NewGuid();

        // --- Window State ---
        public bool AlwaysOnTop { get; set; } = false;
        public bool IsLocked { get; set; } = false;
        public bool ShowToolTip { get; set; } = true;
        public bool IsVertical { get; set; } = false;
        public double Opacity { get; set; } = 1.0;
        public double ScaleFactor { get; set; } = 1.0;

        // --- Measurement Logic ---
        public MeasurementUnit CurrentUnit { get; set; } = MeasurementUnit.Inches;

        /// <summary>
        /// Pixels Per Millimeter. This is the "Master Math" value 
        /// calculated during calibration.
        /// </summary>
        public double Ppm { get; set; } = 3.7795; // Default for 96 DPI

        // --- Magnifier Settings ---
        public bool MagnifierEnabled { get; set; } = false;
        public double MagnifierScale { get; set; } = 2.0;

        // --- Position & Size (DIPs) ---
        public double Left { get; set; } = 100;
        public double Top { get; set; } = 100;
        public double Width { get; set; } = 500;
        public double Height { get; set; } = 70;

        // --- Save Preferences ---
        /// <summary>
        /// Links to your "Save?" menu options (e.g., Auto-save on exit, Manual only).
        /// </summary>
        public int SaveTypeIndex { get; set; } = 0;
    }
}

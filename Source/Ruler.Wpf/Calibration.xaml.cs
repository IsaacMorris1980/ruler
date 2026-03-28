using System;
using System.Windows;

namespace Ruler.Wpf
{
    public partial class CalibrationWindow : Window
    {
        // ISO/IEC 7810 ID-1 standard dimensions in millimeters
        private const double RealCardWidthMm = 85.60;
        private const double RealCardHeightMm = 53.98;

        /// <summary>
        /// The calculated Pixels Per Millimeter based on the user's calibration.
        /// Access this after ShowDialog() returns true.
        /// </summary>
        public double PixelsPerMm { get; private set; }

        public CalibrationWindow()
        {
            InitializeComponent();
            UpdateCardSize();
        }

        private void WidthSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (VirtualCard != null)
            {
                UpdateCardSize();
            }
        }

        private void UpdateCardSize()
        {
            double newWidth = WidthSlider.Value;
            VirtualCard.Width = newWidth;

            // Maintain the 1.585:1 aspect ratio of a credit card
            VirtualCard.Height = (newWidth * RealCardHeightMm) / RealCardWidthMm;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // The formula: The number of pixels used to represent 85.6mm
            PixelsPerMm = VirtualCard.Width / RealCardWidthMm;

            // Log or Store the result
            // Example: Settings.Default.PPM = PixelsPerMm;

            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        // Allow dragging the window even without a title bar
        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }
    }
}
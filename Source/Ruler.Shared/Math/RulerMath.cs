using System;
namespace Ruler.Shared
{
    public static class RulerMath
    {
        public const double RealCardWidthMm = 85.60;
        public const double RealCardHeightMm = 53.98;
        #region Rotation & Bounding Box Math
        /// <summary>
        /// Calculates the required window size to fit a rotated ruler without clipping.
        /// </summary>
        public static (double Width, double Height) GetWindowBounds(double rulerW, double rulerH, double angle)
        {
            double rad = Math.PI * angle / 180.0;
            double cos = Math.Abs(Math.Cos(rad));
            double sin = Math.Abs(Math.Sin(rad));
            // New width/height of the bounding box
            double newWidth = (rulerW * cos) + (rulerH * sin);
            double newHeight = (rulerW * sin) + (rulerH * cos);
            return (newWidth, newHeight);
        }
        /// <summary>
        /// Translates a screen-space mouse coordinate (X,Y) into a local 
        /// coordinate relative to the un-rotated ruler's origin (0,0).
        /// </summary>
        public static (double X, double Y) GetLogicalPoint(double screenX, double screenY, double centerX, double centerY, double angle)
        {
            // Reverse the rotation angle
            double rad = Math.PI * -angle / 180.0;
            double cos = Math.Cos(rad);
            double sin = Math.Sin(rad);
            // Translate to origin (relative to the rotation center)
            double dx = screenX - centerX;
            double dy = screenY - centerY;
            // Apply rotation matrix
            double logicalX = (dx * cos) - (dy * sin);
            double logicalY = (dx * sin) + (dy * cos);
            // Translate back
            return (logicalX + (centerX), logicalY + (centerY));
        }

        #endregion

        #region Calibration Math
        /// <summary>
        /// Calculates the Pixels Per Millimeter based on the width of the 
        /// calibration card on screen.
        /// </summary>
        public static double CalculatePixelsPerMm(double virtualWidth)
        {
            return virtualWidth / RealCardWidthMm;
        }
        /// <summary>
        /// Converts Pixels/mm into a ScaleFactor relative to the Windows 96 DPI standard.
        /// Use this for the TotalScaleFactor in your ViewModel.
        /// </summary>
        public static double ToScaleFactor(double pixelsPerMm)
        {
            if (pixelsPerMm <= 0) return 1.0;
            // Pixels/mm * 25.4 = Actual DPI
            double actualDpi = pixelsPerMm * 25.4;
            // 96 / Actual DPI = Multiplier for RulerSurface
            return 96.0 / actualDpi;
        }
        #endregion
    }
}

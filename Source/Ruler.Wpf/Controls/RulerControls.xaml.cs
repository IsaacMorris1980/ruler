using Ruler.Wpf.Common;
using Ruler.Wpf.Enums;

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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ruler.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for RulerControls.xaml
    /// </summary>
    public partial class RulerControls : UserControl
    {
        // Conversion constant: 1 Device Independent Pixel (DIP) = 1/96th of an inch
        private const double DipPerInch = 96.0;

        public RulerControls()
        {
            InitializeComponent();
            this.SizeChanged += RulerControl_SizeChanged;
            this.Loaded += (s, e) => DrawRuler();
        }

        // --- DEPENDENCY PROPERTIES (The Control's Public API) ---

        public static readonly DependencyProperty UnitTypeProperty =
            DependencyProperty.Register(nameof(MeasurementUnit), typeof(MeasurementUnit), typeof(RulerControls),
                new PropertyMetadata(MeasurementUnit.Inches, OnRulerPropertyChanged));

        public MeasurementUnit UnitType
        {
            get => (MeasurementUnit)GetValue(UnitTypeProperty);
            set => SetValue(UnitTypeProperty, value);
        }

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(nameof(Orientation), typeof(RulerOrientation), typeof(RulerControls),
                new PropertyMetadata(RulerOrientation.Horizontal, OnRulerPropertyChanged));

        public RulerOrientation Orientation
        {
            get => (RulerOrientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public static readonly DependencyProperty RulerGeometryProperty =
            DependencyProperty.Register(nameof(RulerGeometry), typeof(Geometry), typeof(RulerControls),
                new PropertyMetadata(Geometry.Empty));

        public Geometry RulerGeometry
        {
            get => (Geometry)GetValue(RulerGeometryProperty);
            private set => SetValue(RulerGeometryProperty, value);
        }

        private static void OnRulerPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RulerControls control)
            {
                control.DrawRuler();
            }
        }

        // --- VIEW LOGIC (The only job is to generate the geometry) ---

        private void RulerControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawRuler();
        }

        private void DrawRuler()
        {
            LabelsPanel.Children.Clear();

            // 1. DETERMINE DIMENSIONS AND UNITS
            double length = Orientation == RulerOrientation.Horizontal ? this.ActualWidth : this.ActualHeight;
            double rulerThickness = Orientation == RulerOrientation.Horizontal ? this.ActualHeight : this.ActualWidth;

            if (length <= 0 || rulerThickness <= 0) return;

            double ppu = GetPixelsPerMajorUnit(UnitType); // Pixels Per Unit (e.g., 96 DIPs for 1 Inch)
            double totalUnits = length / ppu;

            // Define base tick heights (relative to ruler thickness)
            double minorTickHeight = rulerThickness * 0.3;
            double mediumTickHeight = rulerThickness * 0.5;
            double majorTickHeight = rulerThickness * 0.8;

            // 2. DETERMINE CUSTOM SUBDIVISION PARAMETERS
            int subdivisions;

            if (UnitType == MeasurementUnit.Pixels)
            {
                // Pixels: 50 subdivisions per 100 DIP major unit = 2 DIP interval (High resolution)
                subdivisions = 50;
            }
            else if (UnitType == MeasurementUnit.Centimeters || UnitType == MeasurementUnit.Millimeters)
            {
                // Metric (Base-10): 10 subdivisions (millimeter marks)
                subdivisions = 10;
            }
            else // Inches, Points, Picas (Base-16/Typographic)
            {
                // Imperial/Typographic: 16 subdivisions (standard 1/16th inch marks)
                subdivisions = 16;
            }

            double smallestTickInterval = ppu / subdivisions;

            // 3. CREATE GEOMETRY (PathGeometry Logic)
            PathGeometry pathGeometry = new PathGeometry();

            for (int i = 0; i <= Math.Ceiling(totalUnits); i++)
            {
                // Iterate through sub-divisions
                for (int j = 0; j < subdivisions; j++)
                {
                    double currentPosition = (i * ppu) + (j * smallestTickInterval);
                    if (currentPosition > length) continue;

                    double tickHeight;
                    string label = null;

                    if (j == 0) // Major Unit Tick (e.g., the 1", 2", 1cm, 2cm, or 100px mark)
                    {
                        tickHeight = majorTickHeight;
                        if (i > 0)
                        {
                            label = UnitType == MeasurementUnit.Pixels ? (i * ppu).ToString() : i.ToString();
                        }
                    }
                    // --- EXPLICIT LOGIC FOR EACH UNIT TYPE ---

                    else if (UnitType == MeasurementUnit.Inches || UnitType == MeasurementUnit.Picas || UnitType == MeasurementUnit.Points)
                    {
                        // 16 Subdivisions (1/16th marks)
                        if (j == 8) // Half Unit (1/2 mark)
                        {
                            tickHeight = mediumTickHeight;
                        }
                        else if (j % 4 == 0) // Quarter Units (1/4, 3/4 marks)
                        {
                            tickHeight = minorTickHeight;
                        }
                        else if (j % 2 == 0) // Eighth Units (1/8, 3/8, 5/8, 7/8 marks)
                        {
                            tickHeight = minorTickHeight * 0.75;
                        }
                        else // Sixteenth Units (1/16th marks)
                        {
                            tickHeight = minorTickHeight * 0.5;
                        }
                    }
                    else if (UnitType == MeasurementUnit.Centimeters || UnitType == MeasurementUnit.Millimeters)
                    {
                        // 10 Subdivisions (1/10th marks or mm marks)
                        if (j == 5) // Half Unit (5mm mark)
                        {
                            tickHeight = mediumTickHeight;
                        }
                        else // Millimeter marks
                        {
                            tickHeight = minorTickHeight * 0.5;
                        }
                    }
                    else if (UnitType == MeasurementUnit.Pixels)
                    {
                        // 50 Subdivisions (2 DIP interval)
                        if (j == 25) // 50 DIP mark (1/2 unit)
                        {
                            tickHeight = mediumTickHeight;
                        }
                        else if (j % 5 == 0) // 10 DIP marks (j=5, 10, 15, 20, etc.)
                        {
                            tickHeight = minorTickHeight;
                        }
                        else // The smaller 2 DIP marks
                        {
                            tickHeight = minorTickHeight * 0.5;
                        }
                    }
                    else
                    {
                        // Fallback for any unhandled case (shouldn't happen)
                        tickHeight = rulerThickness * 0.1;
                    }


                    // Add the line segment to the PathGeometry
                    PathFigure figure = new PathFigure();

                    if (Orientation == RulerOrientation.Horizontal)
                    {
                        // Horizontal: Start at (Position, Thickness - Height) and draw down
                        figure.StartPoint = new Point(currentPosition, rulerThickness - tickHeight);
                        figure.Segments.Add(new LineSegment(new Point(currentPosition, rulerThickness), isStroked: true));

                        // Add Text Label
                        if (label != null)
                        {
                            AddLabel(label, currentPosition + 2, 2);
                        }
                    }
                    else // Vertical
                    {
                        // Vertical: Start at (Thickness - Height, Position) and draw right
                        figure.StartPoint = new Point(rulerThickness - tickHeight, currentPosition);
                        figure.Segments.Add(new LineSegment(new Point(rulerThickness, currentPosition), isStroked: true));

                        // Add Text Label (Rotated)
                        if (label != null)
                        {
                            AddLabel(label, 2, currentPosition + 2, rotate: true);
                        }
                    }

                    pathGeometry.Figures.Add(figure);
                }
            }

            // 4. SET THE RESULT
            RulerGeometry = pathGeometry;
        }

        // --- HELPER METHODS ---

        private double GetPixelsPerMajorUnit(MeasurementUnit unit)
        {
            // Returns the number of DIPs for one major unit.
            switch (unit)
            {
                case MeasurementUnit.Inches:
                    // 1 inch = 96 DIPs
                    return DipPerInch;
                case MeasurementUnit.Centimeters:
                    // 1 cm = 96 / 2.54 DIPs
                    return DipPerInch / 2.54;
                case MeasurementUnit.Millimeters:
                    // 1 mm is the unit. 1 mm = 96 / 25.4 DIPs
                    // We will set the major labeled mark to 10 mm (1 cm) for readability.
                    return (DipPerInch / 25.4) * 10;
                case MeasurementUnit.Pixels:
                    // Major unit for labeling/marking is 100 DIPs
                    return 100.0;
                case MeasurementUnit.Points:
                    // 1 pt = 96 / 72 DIPs. Major unit is 72 points (1 inch)
                    return DipPerInch;
                case MeasurementUnit.Picas:
                    // 1 pica = 16 DIPs (12 points). Major unit is 6 picas (1 inch)
                    return DipPerInch;
                default:
                    return DipPerInch;
            }
        }

        private void AddLabel(string text, double x, double y, bool rotate = false)
        {
            TextBlock textBlock = new TextBlock
            {
                Text = text,
                FontSize = 10,
                Foreground = Brushes.Black,
            };

            if (rotate)
            {
                // Set the rotation point (center of the text block)
                textBlock.RenderTransformOrigin = new Point(0.5, 0.5);
                // Rotate 90 degrees clockwise for vertical ruler labels
                textBlock.RenderTransform = new RotateTransform(90);
                // Adjust position
                Canvas.SetLeft(textBlock, x + textBlock.ActualHeight / 2);
                Canvas.SetTop(textBlock, y);
            }
            else
            {
                Canvas.SetLeft(textBlock, x);
                Canvas.SetTop(textBlock, y);
            }


            LabelsPanel.Children.Add(textBlock);
        }
    }
}

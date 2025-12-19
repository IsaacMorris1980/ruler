using Ruler.Wpf.Enums;
using Ruler.Wpf.Services.Persistence.Strategy;
using Ruler.Wpf.Services.Persistence.Strategy.Units;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Ruler.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for RulerControls.xaml
    /// </summary>
    public partial class RulerControl : UserControl
    {
        private const double DipPerInch = 96.0;
        private IUnitStrategy _unitStrategy = new PixelUnitStrategy();

        public RulerControl()
        {
            InitializeComponent();
            this.Loaded += (s, e) => DrawRuler();
            this.SizeChanged += RulerControl_SizeChanged;
        }

        private void RulerControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawRuler();
        }

        #region Dependency Properties

        public static readonly DependencyProperty UnitTypeProperty =
            DependencyProperty.Register(nameof(UnitType), typeof(MeasurementUnit), typeof(RulerControl),
                new PropertyMetadata(MeasurementUnit.Pixels, OnUnitTypeChanged));

        public MeasurementUnit UnitType
        {
            get { return (MeasurementUnit)GetValue(UnitTypeProperty); }
            set { SetValue(UnitTypeProperty, value); }
        }

        private static void OnUnitTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RulerControl control && e.NewValue is MeasurementUnit newUnit)
            {
                control.SetUnitStrategy(newUnit);
                control.DrawRuler();
            }
        }

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(RulerControl),
                new PropertyMetadata(Orientation.Horizontal, OnOrientationChanged));

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RulerControl control) control.DrawRuler();
        }

        #endregion

        private void SetUnitStrategy(MeasurementUnit unit)
        {
            switch (unit)
            {
                case MeasurementUnit.Inches: _unitStrategy = new InchUnitStrategy(); break;
                case MeasurementUnit.Millimeters: _unitStrategy = new MillimeterUnitStrategy(); break;
                case MeasurementUnit.Centimeters: _unitStrategy = new CentimeterUnitStrategy(); break;
                case MeasurementUnit.Pixels: _unitStrategy = new PixelUnitStrategy(); break;
                case MeasurementUnit.Points: _unitStrategy = new PointUnitStrategy(); break;
                default: _unitStrategy = new PixelUnitStrategy(); break;
            }
        }

        private void DrawRuler()
        {
            if (_unitStrategy == null) SetUnitStrategy(UnitType);

            LabelsPanel.Children.Clear();

            // FIX: Use GeometryGroup instead of a single PathFigure with PolyLineSegment.
            // This prevents the diagonal lines connecting the end of one tick to the start of the next.
            GeometryGroup combinedGeometry = new GeometryGroup();

            double rulerLength = (Orientation == Orientation.Horizontal) ? ActualWidth : ActualHeight;
            double rulerDepth = (Orientation == Orientation.Horizontal) ? ActualHeight : ActualWidth;

            // Draw primary side
            DrawTicksForSide(combinedGeometry, rulerLength, rulerDepth, false);

            // Draw mirrored side if enough space
            if (rulerDepth > 30)
            {
                DrawTicksForSide(combinedGeometry, rulerLength, rulerDepth, true);
            }

            RulerPath.Data = combinedGeometry;
        }

        private void DrawTicksForSide(GeometryGroup geometryGroup, double rulerLength, double rulerDepth, bool isMirrored)
        {
            double step = _unitStrategy.GetSmallestUnitDip(DipPerInch);
            if (step <= 0) step = 1.0;

            for (double i = 0; i < rulerLength; i += step)
            {
                double tickLength = _unitStrategy.GetTickLength(i, DipPerInch, rulerDepth);

                if (tickLength > 0)
                {
                    // Add individual line geometry to the group
                    AddTickLine(geometryGroup, i, tickLength, rulerDepth, isMirrored);

                    // Labels for major ticks
                    if (tickLength >= _unitStrategy.MajorTickLengthDip(rulerDepth))
                    {
                        AddLabel(i, rulerDepth, isMirrored);
                    }
                }
            }
        }

        private void AddTickLine(GeometryGroup group, double position, double length, double rulerDepth, bool isMirrored)
        {
            Point start = GetStartPoint(position, rulerDepth, isMirrored);
            Point end = GetEndPoint(position, length, rulerDepth, isMirrored);

            // Create a separate line geometry for every tick
            group.Children.Add(new LineGeometry(start, end));
        }

        //private void AddLabel(double position, double rulerDepth, bool isMirrored)
        //{
        //    double majorUnitDip = _unitStrategy.GetMajorUnitDip(DipPerInch);
        //    int majorIndex = (int)Math.Round(position / majorUnitDip);

        //    if (majorIndex == 0) return;

        //    string labelText = _unitStrategy.GetLabelText(majorIndex);
        //    double labelMargin = 3.0;
        //    double labelHeightApprox = 12.0;
        //    double majorTickLength = _unitStrategy.MajorTickLengthDip(rulerDepth);

        //    TextBlock label = new TextBlock
        //    {
        //        Text = labelText,
        //        FontSize = Math.Min(9, rulerDepth / 2),
        //        Foreground = Brushes.Black,
        //        FontFamily = new FontFamily("Segoe UI")
        //    };

        //    if (Orientation == Orientation.Horizontal)
        //    {
        //        Canvas.SetLeft(label, position + 2);
        //        if (!isMirrored)
        //            Canvas.SetTop(label, majorTickLength + labelMargin);
        //        else
        //            Canvas.SetTop(label, rulerDepth - majorTickLength - labelHeightApprox - labelMargin);
        //    }
        //    else
        //    {
        //        Canvas.SetTop(label, position + 2);
        //        if (!isMirrored)
        //            Canvas.SetLeft(label, majorTickLength + labelMargin);
        //        else
        //            Canvas.SetLeft(label, rulerDepth - majorTickLength - labelHeightApprox - labelMargin);
        //    }

        //    LabelsPanel.Children.Add(label);
        //}
        private void AddLabel(double position, double rulerDepth, bool isMirrored)
        {
            // 1. CONDITIONAL HIDING: If ruler is less than 90px wide/high, hide the mirrored labels
            if (rulerDepth < 90 && isMirrored)
            {
                return;
            }

            double majorUnitDip = _unitStrategy.GetMajorUnitDip(DipPerInch);
            int majorIndex = (int)Math.Round(position / majorUnitDip);

            // Skip the 0 label to avoid clashing with the corner/edge
            if (majorIndex == 0) return;

            string labelText = _unitStrategy.GetLabelText(majorIndex);
            double majorTickLength = _unitStrategy.MajorTickLengthDip(rulerDepth);

            // 2. CALCULATE DYNAMIC MARGINS
            // If the ruler is narrow (< 90), we give the primary labels more space 
            // since the other side is hidden.
            double labelOffset = 2.0;
            if (rulerDepth < 90)
            {
                labelOffset = 8.0; // Increased space for left/top labels when solitary
            }
            else if (rulerDepth > 150)
            {
                labelOffset = 15.0;
            }
            else if (rulerDepth > 90)
            {
                labelOffset = 5.0;
            }

            TextBlock label = new TextBlock();
            label.Text = labelText;

            // Adjust font size based on depth
            label.FontSize = Math.Min(10, rulerDepth / 4 + 2);
            label.Foreground = Brushes.Black;
            label.FontFamily = new FontFamily("Segoe UI");

            if (Orientation == Orientation.Horizontal)
            {
                // Top/Bottom Labels
                Canvas.SetLeft(label, position + 2);

                if (!isMirrored)
                {
                    // TOP Labels
                    Canvas.SetTop(label, majorTickLength + labelOffset);
                }
                else
                {
                    // BOTTOM Labels (Only runs if depth >= 90)
                    Canvas.SetTop(label, rulerDepth - majorTickLength - labelOffset - 14);
                }
            }
            else
            {
                // Left/Right Labels
                Canvas.SetTop(label, position + 2);

                if (!isMirrored)
                {
                    // LEFT Labels
                    Canvas.SetLeft(label, majorTickLength + labelOffset);
                }
                else
                {
                    // RIGHT Labels (Only runs if depth >= 90)
                    Canvas.SetLeft(label, rulerDepth - majorTickLength - labelOffset - 25);
                }
            }

            LabelsPanel.Children.Add(label);
        }

        private Point GetStartPoint(double position, double rulerDepth, bool isMirrored)
        {
            if (Orientation == Orientation.Horizontal)
                return new Point(position, isMirrored ? rulerDepth : 0);
            else
                return new Point(isMirrored ? rulerDepth : 0, position);
        }

        private Point GetEndPoint(double position, double length, double rulerDepth, bool isMirrored)
        {
            if (Orientation == Orientation.Horizontal)
                return new Point(position, isMirrored ? rulerDepth - length : length);
            else
                return new Point(isMirrored ? rulerDepth - length : length, position);
        }
    }
}
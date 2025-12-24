using Ruler.Wpf.Enums;
using Ruler.Wpf.Services.Persistence.Strategy;
using Ruler.Wpf.Services.Persistence.Strategy.Units;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
            this.SizeChanged += (s,  e) => {
                DrawRuler();
            };
            this.Loaded += (s, e) => {              
                DrawRuler();
            };
       
        }
        #region Dependency Properties

        public static readonly DependencyProperty ScaleFactorProperty =
           DependencyProperty.Register(nameof(ScaleFactor), typeof(double), typeof(RulerControl),
               new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender, OnRulerPropertyChanged));

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(RulerControl),
                new FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsRender, OnRulerPropertyChanged));

        public static readonly DependencyProperty UnitTypeProperty =
            DependencyProperty.Register(nameof(UnitType), typeof(MeasurementUnit), typeof(RulerControl),
                new FrameworkPropertyMetadata(MeasurementUnit.Inches, FrameworkPropertyMetadataOptions.AffectsRender, OnRulerPropertyChanged));


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
        private static void OnRulerPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs baseValue)
        {
            if (d is RulerControl control)
            {
                if (d.GetValue(UnitTypeProperty) is MeasurementUnit newUnit)
                {
                    control.SetUnitStrategy(newUnit);
                }
                else if (d.GetValue(ScaleFactorProperty) is double newScale)
                {
                    control.SetScaleFactor(newScale);
                }
                else if (d.GetValue(OrientationProperty) is Orientation newOrientation)
                {
                    control.SetOrientation(newOrientation);
                }
                control.DrawRuler();
            }
           
        }


        private double ScaleFactor
        {
            get { return (double)GetValue(ScaleFactorProperty); }
            set { SetValue(ScaleFactorProperty, value); }
        }

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }
        private void SetOrientation(Orientation orientation)
        {
            Orientation = orientation;
        }
        private void SetScaleFactor(double scale)
        {
            ScaleFactor = scale;
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
            if (LabelsPanel == null || RulerPath == null) return;

            LabelsPanel.Children.Clear();
            double depth = Orientation == Orientation.Horizontal ? ActualHeight : ActualWidth;
            double length = Orientation == Orientation.Horizontal ? ActualWidth : ActualHeight;

            if (depth <= 0 || length <= 0) return;

            GeometryGroup group = new GeometryGroup();
            double effectiveDpi = DipPerInch * ScaleFactor;

            double smallestUnitDip = _unitStrategy.GetSmallestUnitDip(effectiveDpi);
            double majorUnitDip = _unitStrategy.GetMajorUnitDip(effectiveDpi);

            if (smallestUnitDip <= 0) return;

            RulerPath.SnapsToDevicePixels = true;

            // FIX: Use an integer loop for ticks to avoid cumulative floating point errors
            int totalSteps = (int)Math.Ceiling(length / smallestUnitDip);

            for (int i = 0; i <= totalSteps; i++)
            {
                double pos = i * smallestUnitDip;
                if (pos > length + 0.001) break;

                double tickLen = _unitStrategy.GetTickLength(pos, effectiveDpi, depth);

                group.Children.Add(new LineGeometry(GetStartPoint(pos, depth, false), GetEndPoint(pos, tickLen, depth, false)));

                if (depth > 40)
                    group.Children.Add(new LineGeometry(GetStartPoint(pos, depth, true), GetEndPoint(pos, tickLen, depth, true)));

                // FIX: Check if this step corresponds to a Major Unit (e.g. 1cm, 2cm)
                // We calculate how many "smallest units" fit into one "major unit"
                double stepsPerMajor = majorUnitDip / smallestUnitDip;

                // Use a small epsilon check on the step index to identify major marks
                if (Math.Abs(i % stepsPerMajor) < 0.001 || Math.Abs((i % stepsPerMajor) - stepsPerMajor) < 0.001)
                {
                    int majorIndex = (int)Math.Round(i / stepsPerMajor);
                    if (majorIndex > 0)
                    {
                        AddLabel(pos, majorIndex);
                    }
                }

                RulerPath.Data = group;
            }
        }
            //double availableWidth = ActualWidth;
            //double availableHeight = ActualHeight;          
            //if (_unitStrategy == null) SetUnitStrategy(UnitType);
            //if (LabelsPanel == null || RulerPath == null || availableWidth <= 0 || availableHeight <= 0) return;
            //LabelsPanel.Children.Clear();

            //bool isHorizontal = Orientation == Orientation.Horizontal;
            //double totalLength = isHorizontal ? availableWidth : availableHeight;
            //double totalDepth = isHorizontal ? availableHeight : availableWidth;


            //// FIX: Use GeometryGroup instead of a single PathFigure with PolyLineSegment.
            //// This prevents the diagonal lines connecting the end of one tick to the start of the next.
            //GeometryGroup combinedGeometry = new GeometryGroup();
            //double effectiveDpi = DipPerInch * ScaleFactor;

            //double step = _unitStrategy.GetSmallestUnitDip(effectiveDpi);
            //double majorUnitDip = _unitStrategy.GetMajorUnitDip(effectiveDpi);

            //if (step <= 0) return;
            //double offset = 0.5;

            //if (totalLength <= 0 || totalDepth <= 0) return;

            //for (double pos = 0; pos <= totalLength; pos += step)
            //{
            //    double tickLen = _unitStrategy.GetTickLength(pos, effectiveDpi, totalDepth);

            //    combinedGeometry.Children.Add(CreateTickGeometry(pos, tickLen, totalDepth, offset, false));

            //    if (totalDepth > 40)
            //        combinedGeometry.Children.Add(CreateTickGeometry(pos, tickLen, totalDepth, offset, true));

            //    if (Math.Abs(pos % majorUnitDip) < 0.001)
            //    {
            //        AddLabel(pos, tickLen, totalDepth, isHorizontal, effectiveDpi);
            //    }
            //}

            //RulerPath.Data = combinedGeometry;

            //for (double i = 0; i <= totalLength; i += step)
            //{
            //    double tickLength = _unitStrategy.GetTickLength(i, DipPerInch, totalDepth);

            //    if (tickLength > 0)
            //    {
            //        // Primary side tick
            //        combinedGeometry.Children.Add(new LineGeometry(
            //            GetStartPoint(i, rulerDepth, false),
            //            GetEndPoint(i, tickLength, rulerDepth, false)));

            //        // Mirrored side tick (opposite edge)
            //        combinedGeometry.Children.Add(new LineGeometry(
            //            GetStartPoint(i, rulerDepth, true),
            //            GetEndPoint(i, tickLength, rulerDepth, true)));

            //        // Add Labels for Major Units
            //        if (Math.Abs(i % majorUnitDip) < 0.001 || Math.Abs((i % majorUnitDip) - majorUnitDip) < 0.001)
            //        {
            //            AddLabel(i, rulerDepth, false);

            //            // Only add mirrored labels if there is enough space
            //            if (rulerDepth >= 90)
            //            {
            //                AddLabel(i, rulerDepth, true);
            //            }
            //        }
            //    }
            //}

            //   RulerPath.Data = combinedGeometry;
            //// Draw primary side
            //DrawTicksForSide(combinedGeometry, rulerLength, rulerDepth, false);

            //// Draw mirrored side if enough space
            //if (rulerDepth > 30)
            //{
            //    DrawTicksForSide(combinedGeometry, rulerLength, rulerDepth, true);
            //}

            //RulerPath.Data = combinedGeometry;
        
        private LineGeometry CreateTickGeometry(double pos, double tickLen, double depth, double offset, bool mirrored)
        {
            if (Orientation == Orientation.Horizontal)
            {
                double y1 = mirrored ? depth - offset : offset;
                double y2 = mirrored ? depth - tickLen : tickLen;
                return new LineGeometry(new Point(pos + offset, y1), new Point(pos + offset, y2));
            }
            else
            {
                double x1 = mirrored ? depth - offset : offset;
                double x2 = mirrored ? depth - tickLen : tickLen;
                return new LineGeometry(new Point(x1, pos + offset), new Point(x2, pos + offset));
            }
        }

        //private void AddLabel(double pos, double tickLen, double depth, bool isHorizontal, double dpi)
        //{
        //    double majorUnitDip = _unitStrategy.GetMajorUnitDip(dpi);
        //    int index = (int)Math.Round(pos / majorUnitDip);

        //    TextBlock label = new TextBlock
        //    {
        //        Text = _unitStrategy.GetLabelText(index),
        //        FontSize = 10,
        //        Foreground = Brushes.Black,
        //        IsHitTestVisible = false
        //    };

        //    if (isHorizontal)
        //    {
        //        Canvas.SetLeft(label, pos + 2);
        //        Canvas.SetTop(label, tickLen + 2);
        //    }
        //    else
        //    {
        //        Canvas.SetTop(label, pos + 2);
        //        Canvas.SetLeft(label, tickLen + 2);
        //        label.RenderTransform = new RotateTransform(90);
        //    }

        //    LabelsPanel.Children.Add(label);
        //}

        //private void AddLabel(double position, double rulerDepth, bool isMirrored)
        //{
        //    double majorUnitDip = _unitStrategy.GetMajorUnitDip(DipPerInch);
        //    int majorIndex = (int)Math.Round(position / majorUnitDip);

        //    if (majorIndex == 0) return;

        //    string labelText = _unitStrategy.GetLabelText(majorIndex);
        //    double majorTickLength = _unitStrategy.MajorTickLengthDip(rulerDepth);

        //    // Dynamic offset calculation based on depth
        //    double labelOffset = rulerDepth < 90 ? 8.0 : (rulerDepth > 150 ? 15.0 : 5.0);

        //    TextBlock label = new TextBlock
        //    {
        //        Text = labelText,
        //        FontSize = Math.Max(8, Math.Min(10, rulerDepth / 4 + 2)),
        //        Foreground = Brushes.Black,
        //        FontFamily = new FontFamily("Segoe UI")
        //    };

        //    if (Orientation == Orientation.Horizontal)
        //    {
        //        Canvas.SetLeft(label, position + 2);
        //        if (!isMirrored)
        //            Canvas.SetTop(label, majorTickLength + labelOffset);
        //        else
        //            Canvas.SetTop(label, rulerDepth - majorTickLength - labelOffset - 14);
        //    }
        //    else
        //    {
        //        Canvas.SetTop(label, position + 2);
        //        if (!isMirrored)
        //            Canvas.SetLeft(label, majorTickLength + labelOffset);
        //        else
        //            Canvas.SetLeft(label, rulerDepth - majorTickLength - labelOffset - 25);
        //    }

        //    LabelsPanel.Children.Add(label);
        //}

        //private Point GetStartPoint(double position, double rulerDepth, bool isMirrored)
        //{
        //    if (Orientation == Orientation.Horizontal)
        //        return new Point(position, isMirrored ? rulerDepth : 0);
        //    return new Point(isMirrored ? rulerDepth : 0, position);
        //}

        //private Point GetEndPoint(double position, double length, double rulerDepth, bool isMirrored)
        //{
        //    if (Orientation == Orientation.Horizontal)
        //    {
        //        return isMirrored ? new Point(position, rulerDepth - length) : new Point(position, length);
        //    }
        //    return isMirrored ? new Point(rulerDepth - length, position) : new Point(length, position);
        //}

        private void AddLabel(double position, int majorIndex)
        {
            TextBlock label = new TextBlock
            {
                Text = _unitStrategy.GetLabelText(majorIndex),
                FontSize = 10,
                Foreground = Brushes.Black,
                IsHitTestVisible = false
            };

            if (Orientation == Orientation.Horizontal)
            {
                Canvas.SetLeft(label, position + 2);
                Canvas.SetTop(label, 18);
            }
            else
            {
                Canvas.SetTop(label, position + 2);
                Canvas.SetLeft(label, 18);
                label.RenderTransform = new RotateTransform(90);
            }
            LabelsPanel.Children.Add(label);
        }

        private Point GetStartPoint(double position, double rulerDepth, bool isMirrored)
        {
            double offset = 0.5;
            return (Orientation == Orientation.Horizontal)
                ? new Point(position + offset, isMirrored ? rulerDepth - offset : offset)
                : new Point(isMirrored ? rulerDepth - offset : offset, position + offset);
        }

        private Point GetEndPoint(double position, double length, double rulerDepth, bool isMirrored)
        {
            double offset = 0.5;
            if (Orientation == Orientation.Horizontal)
                return isMirrored ? new Point(position + offset, rulerDepth - length) : new Point(position + offset, length);
            return isMirrored ? new Point(rulerDepth - length, position + offset) : new Point(length, position + offset);
        }
    }
}
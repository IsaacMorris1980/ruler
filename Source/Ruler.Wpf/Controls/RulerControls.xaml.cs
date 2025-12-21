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
       
        private bool _isGuidelineLocked = false;
        private Rectangle _guideLine;

        public RulerControl()
        {
            InitializeComponent();
            this.Loaded += (s, e) => DrawRuler();
            this.SizeChanged += RulerControl_SizeChanged;
            this.Loaded += (s, e) => {              
                DrawRuler();
            };
            //this.MouseMove += RulerControl_MouseMove;
            //this.MouseLeave += RulerControl_MouseLeave;
            //this.MouseLeftButtonDown += RulerControl_MouseLeftButtonDown;
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
            if(LabelsPanel == null || RulerPath == null) return;
            if (_unitStrategy == null) SetUnitStrategy(UnitType);

            LabelsPanel.Children.Clear();

            // FIX: Use GeometryGroup instead of a single PathFigure with PolyLineSegment.
            // This prevents the diagonal lines connecting the end of one tick to the start of the next.
            GeometryGroup combinedGeometry = new GeometryGroup();

            double rulerLength = (Orientation == Orientation.Horizontal) ? ActualWidth : ActualHeight;
            double rulerDepth = (Orientation == Orientation.Horizontal) ? ActualHeight : ActualWidth;
            if (rulerLength <= 0 || rulerDepth <= 0) return;
            double step = _unitStrategy.GetSmallestUnitDip(DipPerInch);
            double majorUnitDip = _unitStrategy.GetMajorUnitDip(DipPerInch);
            if (step <= 0) step = 1.0;

            for (double i = 0; i <= rulerLength; i += step)
            {
                double tickLength = _unitStrategy.GetTickLength(i, DipPerInch, rulerDepth);

                if (tickLength > 0)
                {
                    // Primary side tick
                    combinedGeometry.Children.Add(new LineGeometry(
                        GetStartPoint(i, rulerDepth, false),
                        GetEndPoint(i, tickLength, rulerDepth, false)));

                    // Mirrored side tick (opposite edge)
                    combinedGeometry.Children.Add(new LineGeometry(
                        GetStartPoint(i, rulerDepth, true),
                        GetEndPoint(i, tickLength, rulerDepth, true)));

                    // Add Labels for Major Units
                    if (Math.Abs(i % majorUnitDip) < 0.001 || Math.Abs((i % majorUnitDip) - majorUnitDip) < 0.001)
                    {
                        AddLabel(i, rulerDepth, false);

                        // Only add mirrored labels if there is enough space
                        if (rulerDepth >= 90)
                        {
                            AddLabel(i, rulerDepth, true);
                        }
                    }
                }
            }

            RulerPath.Data = combinedGeometry;
            //// Draw primary side
            //DrawTicksForSide(combinedGeometry, rulerLength, rulerDepth, false);

            //// Draw mirrored side if enough space
            //if (rulerDepth > 30)
            //{
            //    DrawTicksForSide(combinedGeometry, rulerLength, rulerDepth, true);
            //}

            //RulerPath.Data = combinedGeometry;
        }

        private void AddLabel(double position, double rulerDepth, bool isMirrored)
        {
            double majorUnitDip = _unitStrategy.GetMajorUnitDip(DipPerInch);
            int majorIndex = (int)Math.Round(position / majorUnitDip);

            if (majorIndex == 0) return;

            string labelText = _unitStrategy.GetLabelText(majorIndex);
            double majorTickLength = _unitStrategy.MajorTickLengthDip(rulerDepth);

            // Dynamic offset calculation based on depth
            double labelOffset = rulerDepth < 90 ? 8.0 : (rulerDepth > 150 ? 15.0 : 5.0);

            TextBlock label = new TextBlock
            {
                Text = labelText,
                FontSize = Math.Max(8, Math.Min(10, rulerDepth / 4 + 2)),
                Foreground = Brushes.Black,
                FontFamily = new FontFamily("Segoe UI")
            };

            if (Orientation == Orientation.Horizontal)
            {
                Canvas.SetLeft(label, position + 2);
                if (!isMirrored)
                    Canvas.SetTop(label, majorTickLength + labelOffset);
                else
                    Canvas.SetTop(label, rulerDepth - majorTickLength - labelOffset - 14);
            }
            else
            {
                Canvas.SetTop(label, position + 2);
                if (!isMirrored)
                    Canvas.SetLeft(label, majorTickLength + labelOffset);
                else
                    Canvas.SetLeft(label, rulerDepth - majorTickLength - labelOffset - 25);
            }

            LabelsPanel.Children.Add(label);
        }

        private Point GetStartPoint(double position, double rulerDepth, bool isMirrored)
        {
            if (Orientation == Orientation.Horizontal)
                return new Point(position, isMirrored ? rulerDepth : 0);
            return new Point(isMirrored ? rulerDepth : 0, position);
        }

        private Point GetEndPoint(double position, double length, double rulerDepth, bool isMirrored)
        {
            if (Orientation == Orientation.Horizontal)
            {
                return isMirrored ? new Point(position, rulerDepth - length) : new Point(position, length);
            }
            return isMirrored ? new Point(rulerDepth - length, position) : new Point(length, position);
        }

     
    }
}
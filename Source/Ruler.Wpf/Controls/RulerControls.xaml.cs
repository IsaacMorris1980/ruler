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
    /// A high-performance ruler control that uses a Strategy pattern to handle 
    /// different measurement units (Pixels, Inches, CM, etc.).
    /// </summary>
    public partial class RulerControl : UserControl
    {
        private const double BaseDpi = 96.0;
        private IUnitStrategy _unitStrategy = new InchUnitStrategy();

        #region Dependency Properties

        /// <summary>
        /// The type of measurement unit to display.
        /// Fixed: Explicit cast (MeasurementUnit) prevents type mismatch ArgumentException.
        /// </summary>
        public static readonly DependencyProperty UnitTypeProperty =
            DependencyProperty.Register(
                nameof(UnitType),
                typeof(MeasurementUnit),
                typeof(RulerControl),
                new FrameworkPropertyMetadata(
                    (MeasurementUnit)MeasurementUnit.Pixels,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    OnRulerPropertyChanged));

        public MeasurementUnit UnitType
        {
            get => (MeasurementUnit)GetValue(UnitTypeProperty);
            set => SetValue(UnitTypeProperty, value);
        }

        /// <summary>
        /// Combined scaling factor (System DPI * Application Zoom).
        /// </summary>
        public static readonly DependencyProperty ScaleFactorProperty =
            DependencyProperty.Register(
                nameof(ScaleFactor),
                typeof(double),
                typeof(RulerControl),
                new FrameworkPropertyMetadata(
                    1.0,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    OnRulerPropertyChanged));

        public double ScaleFactor
        {
            get => (double)GetValue(ScaleFactorProperty);
            set => SetValue(ScaleFactorProperty, value);
        }

        #endregion

        public RulerControl()
        {
            InitializeComponent();
            this.Loaded += (s, e) => 
            {
                UpdateStrategy();
                DrawRuler();
            };
            this.SizeChanged += (s, e) => DrawRuler();
        }

        private static void OnRulerPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RulerControl control)
            {
                control.UpdateStrategy();
                control.DrawRuler();
            }
        }

        /// <summary>
        /// Updates the internal strategy based on the UnitType property.
        /// </summary>
        private void UpdateStrategy()
        {
            switch (UnitType)
            {
                case MeasurementUnit.Inches:
                    _unitStrategy = new InchUnitStrategy();
                    break;
                case MeasurementUnit.Centimeters:
                    _unitStrategy = new CentimeterUnitStrategy();
                    break;
                case MeasurementUnit.Millimeters:
                    _unitStrategy = new MillimeterUnitStrategy();
                    break;
                case MeasurementUnit.Points:
                    _unitStrategy = new PointUnitStrategy();
                    break;
                case MeasurementUnit.Pixels:
                default:
                    _unitStrategy = new PixelUnitStrategy();
                    break;
            }

            // Sync the static scale factor for compensated strategies
            if (_unitStrategy is PixelUnitStrategy) PixelUnitStrategy.SystemScaleFactor = ScaleFactor;
            if (_unitStrategy is PointUnitStrategy) PointUnitStrategy.SystemScaleFactor = ScaleFactor;
        }

        /// <summary>
        /// Main drawing loop. Generates a GeometryGroup for the Path and populates Labels.
        /// Logic assumes a Horizontal orientation; MainWindow's LayoutTransform handles rotation.
        /// </summary>
        public void DrawRuler()
        {
            // RulerPath and LabelsPanel are expected to be defined in RulerControl.xaml
            if (RulerPath == null || LabelsPanel == null || ActualWidth <= 0 || ActualHeight <= 0)
                return;

            LabelsPanel.Children.Clear();

            double length = ActualWidth;
            double depth = ActualHeight;

            GeometryGroup group = new GeometryGroup();
            double effectiveDpi = BaseDpi * (ScaleFactor > 0 ? ScaleFactor : 1.0);

            double smallestUnit = _unitStrategy.GetSmallestUnitDip(effectiveDpi);
            double majorUnitDip = _unitStrategy.GetMajorUnitDip(effectiveDpi);

            if (smallestUnit <= 0) return;

            int majorIndex = 0;
            for (double pos = 0; pos <= length; pos += smallestUnit)
            {
                double tickLen = _unitStrategy.GetTickLength(pos, effectiveDpi, depth);

                // Top ticks
                group.Children.Add(new LineGeometry(
                    new Point(pos + 0.5, 0),
                    new Point(pos + 0.5, tickLen)));

                // Bottom ticks (if ruler depth allows for double-sided display)
                if (depth > 40)
                {
                    group.Children.Add(new LineGeometry(
                        new Point(pos + 0.5, depth),
                        new Point(pos + 0.5, depth - tickLen)));
                }

                // Check if we are at a major unit position to add a label
                // Using a small epsilon to handle floating point precision
                if (Math.Abs(pos % majorUnitDip) < (smallestUnit / 2.0))
                {
                    AddLabel(pos, tickLen, depth, majorIndex);
                    majorIndex++;
                }
            }

            RulerPath.Data = group;
        }

        private void AddLabel(double pos, double tickLen, double depth, int majorIndex)
        {
            var label = new TextBlock
            {
                Text = _unitStrategy.GetLabelText(majorIndex),
                FontSize = 10,
                Foreground = Brushes.Black,
                IsHitTestVisible = false
            };

            // Positioning: Slightly offset from the major tick
            Canvas.SetLeft(label, pos + 2);
            Canvas.SetTop(label, tickLen + 1);
            LabelsPanel.Children.Add(label);
        }
    }
}
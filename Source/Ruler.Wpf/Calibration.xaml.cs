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
using System.Windows.Shapes;

namespace Ruler.Wpf
{
    /// <summary>
    /// Interaction logic for Calibration.xaml
    /// </summary>
    public partial class Calibaration : Window
    {
        // Calibration State
        private int _currentStep = 0;
        private readonly List<Point> _screenPoints = new List<Point>();
        private readonly List<Point> _machinePoints = new List<Point>();

        // Define the target points on the screen (corners and center)
        private Point[] _targetPositions;

        public Calibaration()
        {
            InitializeComponent();
            this.Loaded += CalibrationWindow_Loaded;
        }

        private void CalibrationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Enter full screen for accurate calibration
            this.WindowState = WindowState.Maximized;
            this.WindowStyle = WindowStyle.None;
            this.Topmost = true;

            DefineTargetPoints();
            UpdateUI();
        }

        private void DefineTargetPoints()
        {
            double margin = 50;
            double w = SystemParameters.PrimaryScreenWidth;
            double h = SystemParameters.PrimaryScreenHeight;

            // Define 5-point calibration (Top-Left, Top-Right, Bottom-Right, Bottom-Left, Center)
            _targetPositions = new Point[]
            {
                new Point(margin, margin),
                new Point(w - margin, margin),
                new Point(w - margin, h - margin),
                new Point(margin, h - margin),
                new Point(w / 2, h / 2)
            };
        }

        private void UpdateUI()
        {
            if (_currentStep < _targetPositions.Length)
            {
                // Position the target visual (the crosshair)
                Point nextPos = _targetPositions[_currentStep];
                Canvas.SetLeft(CalibrationTarget, nextPos.X - (CalibrationTarget.ActualWidth / 2));
                Canvas.SetTop(CalibrationTarget, nextPos.Y - (CalibrationTarget.ActualHeight / 2));

                StatusText.Text = $"Click the target (Step {_currentStep + 1} of {_targetPositions.Length})";
                InstructionText.Text = "Please tap the center of the crosshair precisely.";
            }
            else
            {
                FinishCalibration();
            }
        }

        private void OnTargetMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Capture the exact point clicked
            Point clickedPoint = e.GetPosition(this);
            _screenPoints.Add(clickedPoint);

            // Here you would typically grab the current coordinates from your 
            // motor controller or hardware device
            // _machinePoints.Add(Hardware.GetCurrentPos()); 

            _currentStep++;
            UpdateUI();
        }

        private void FinishCalibration()
        {
            // Calculate the transformation matrix or offsets here
            // This is where you would save the data to .NET Settings or a file

            StatusText.Text = "Calibration Complete!";
            InstructionText.Text = "Calculating transformation matrix...";

            // Logic to save coefficients
            SaveCalibrationData();

            // Briefly show success then close
            var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            timer.Tick += (s, e) => {
                timer.Stop();
                this.DialogResult = true;
                this.Close();
            };
            timer.Start();
        }

        private void SaveCalibrationData()
        {
            // Example of how you might persist these values
            // Properties.Settings.Default.CalibrationData = SerializedData;
            // Properties.Settings.Default.Save();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape) CloseButton_Click(null, null);
            base.OnKeyDown(e);
        }
    }
}

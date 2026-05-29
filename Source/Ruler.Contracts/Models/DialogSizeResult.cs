using Ruler.Contracts.Enums;

namespace Ruler.Contracts.Models
{
    public struct DialogSizeResult
    {
        public double Width { get; }
        public double Height { get; }
        public MeasurementUnit SelectedUnit { get; }
        public DialogSizeResult(double width, double height, MeasurementUnit selectedUnit)
        {
            Width = width;
            Height = height;
            SelectedUnit = selectedUnit;
        }
    }
}

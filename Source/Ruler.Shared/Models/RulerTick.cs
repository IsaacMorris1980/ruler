namespace Ruler.Shared
{
    public class RulerTick
    {
        public string Label { get; set; }
        public double Position { get; set; }
        public double TickSize { get; set; }
        public bool HasLabel => !string.IsNullOrEmpty(Label);
        public bool IsLabelVisible { get; set; }
    }
}

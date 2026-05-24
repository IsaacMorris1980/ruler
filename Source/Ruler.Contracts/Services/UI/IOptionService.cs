using System.Collections.ObjectModel;

namespace Ruler.Shared
{
    public interface IOptionService
    {
       ObservableCollection<UnitOption> Units { get; }
       ObservableCollection<OpacityOption> Opacities { get; }
       ObservableCollection<ScaleOption> Scales { get; }
       ObservableCollection<SaveOption> SaveOptions { get; }
    }
}

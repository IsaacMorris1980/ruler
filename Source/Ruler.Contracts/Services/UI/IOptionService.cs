using System.Collections.ObjectModel;
using Ruler.Contracts.Models;

namespace Ruler.Contracts.Services.UI

public interface IOptionService
{
   ObservableCollection<UnitOption> Units { get; }
   ObservableCollection<OpacityOption> Opacities { get; }
   ObservableCollection<ScaleOption> Scales { get; }
   ObservableCollection<SaveOption> SaveOptions { get; }
}

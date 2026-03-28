using Ruler.Wpf.Models;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ruler.Shared.Models;

namespace Ruler.Shared.Interfaces
{
    public interface IOptionService
    {
       ObservableCollection<UnitOption> Units { get; }
       ObservableCollection<OpacityOption> Opacities { get; }
       ObservableCollection<ScaleOption> Scales { get; }
       ObservableCollection<SaveOption> SaveOptions { get; }
    }
}

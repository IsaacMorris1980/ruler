using Ruler.Wpf.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Wpf.Services.Persistence.Strategy
{ 
    public interface IPersistenceStrategy
    {
        void Save<T>(List<T> rulerInfo);
        List<T> Load<T>();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Interfaces
{
    // 2. An interface for "Preprocessing" data (e.g., the RulerInfo stripping logic)
    public interface IDataPreprocessor<T>
    {
        IEnumerable<T> Preprocess(IEnumerable<T> items);
    }
}

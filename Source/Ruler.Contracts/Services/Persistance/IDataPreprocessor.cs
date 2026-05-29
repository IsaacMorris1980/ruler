using System.Collections.Generic;

namespace Ruler.Contracts.Services.Persistance
{
    // 2. An interface for "Preprocessing" data (e.g., the RulerInfo stripping logic)
    public interface IDataPreprocessor<T>
    {
        IEnumerable<T> Preprocess(IEnumerable<T> items);
    }
}
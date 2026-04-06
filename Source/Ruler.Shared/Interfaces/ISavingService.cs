using System.Collections.Generic;

namespace Ruler.Shared
{
    public interface ISavingService
    {
        void Save<T>(List<T> items);
        List<T> Load<T>();
    }
}
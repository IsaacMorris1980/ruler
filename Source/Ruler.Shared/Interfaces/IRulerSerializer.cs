using Ruler.Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Interfaces
{
    public interface IRulerSerializer
    {
        List<RulerInfo> DeserializeRulers(string rulersToDecrypt);
        string SerializeRulers(IEnumerable<RulerInfo> rulers);
    }
}

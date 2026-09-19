using Ruler.Shared.Models;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Interfaces
{
    public interface IMainFormFactory
    {
        IRuler Create(RulerInfo info, EventHandler handler);
    }
}

using Ruler.Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Interfaces
{
    public interface IRulerRegistry
    {
        void Register(IMainViewModel ruler);

        void Unregister(IMainViewModel ruler);

        IEnumerable<IMainViewModel> GetActiveRulers();

        // Retrieves a specific ruler by its unique ID
        IMainViewModel   GetRulerById(Guid id);

        // Closes and unregisters all active rulers
        void CloseAll();
        void Clear();
        bool RulerExists(Guid rulerID);
        void UpdateRulerByID(RulerInfo rulerInfo);
    }
}

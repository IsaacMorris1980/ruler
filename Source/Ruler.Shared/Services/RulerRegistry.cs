using Ruler.Shared.Interfaces;
using Ruler.Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Shared.Services
{
    public class RulerRegistry : IRulerRegistry
    {
        private readonly Dictionary<Guid, IMainViewModel> _rulers = new Dictionary<Guid, IMainViewModel>();
        private readonly object _lock = new object();

        public void Register(IMainViewModel ruler)
        {
            lock (_lock)
            {
                if (!_rulers.ContainsKey(ruler.RulerData.ID))
                {
                    _rulers.Add(ruler.RulerData.ID, ruler);
                }
            }
        }

        public void Unregister(IMainViewModel ruler)
        {
            lock (_lock)
            {
                if (_rulers.ContainsKey(ruler.RulerData.ID))
                {
                    _rulers.Remove(ruler.RulerData.ID);
                }
            }
        }

        public IEnumerable<IMainViewModel> GetActiveRulers()
        {
            lock (_lock)
            {
                return _rulers.Values.ToList();
            }
        }
        public bool RulerExists(Guid rulerID)
        {
            bool found = false;
            lock (_lock)
            {
                if (_rulers.TryGetValue(rulerID, out var ruler))
                {
                    found = true;
                }

            }
            return found;
        }
        public void UpdateRulerByID(RulerInfo rulerInfo)
        {
            if (RulerExists(rulerInfo.ID))
            {
                var ruler = GetRulerById(rulerInfo.ID);
 //               ruler.SetRulerInfo(rulerInfo);
            }
        }

        public IMainViewModel GetRulerById(Guid id)
        {
            lock (_lock)
            {
                _rulers.TryGetValue(id, out var ruler);
                return ruler;
            }
        }

        public void CloseAll()
        {
            List<IMainViewModel> toClose;
            lock (_lock)
            {
                toClose = _rulers.Values.ToList();
            }

            foreach (var ruler in toClose)
            {
                ruler.RequestClose(); // This should ideally unregister itself via the Closed event
            }
        }
        public void Clear()
        {
            // Close all forms currently in memory
            foreach (var ruler in _rulers.ToList())
            {
                var form = ruler.Value as IMainViewModel;
                form?.RequestClose(); // This triggers your OnFormClosed logic
            }
            _rulers.Clear();
        }
    }
}

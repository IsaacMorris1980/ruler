using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ruler.Shared.Models;
using Ruler.Shared.Services;

namespace Ruler.Shared.ViewModels
{
    public class RulerViewModel : ViewModelBase
    {
        private RulerInfo _rulerInfo;
        private IUpdateService _updateService;
        private IRulerFactory _rulerFactory;
        public RulerViewModel(RulerInfo ruler, IRulerFactory rulerFactory, IUpdateService updateService)
        {
            _rulerFactory = rulerFactory;
            _updateService = updateService;
            _rulerInfo = ruler;
        }
    }
}

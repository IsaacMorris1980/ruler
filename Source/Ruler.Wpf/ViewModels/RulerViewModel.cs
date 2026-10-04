using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ruler.Shared.Models;
using Ruler.Shared.Services;

namespace Ruler.Wpf.ViewModels
{
    public class RulerViewModel : ViewModelBase
    {
        private RulerInfo _rulerInfo;
        private IRulerFactory _rulerFactory;
        public RulerViewModel(RulerInfo ruler,  IRulerFactory rulerFactory)
        {
            _rulerFactory = rulerFactory;
            _rulerInfo = ruler;
        }
        public RulerInfo RulerData
        {
            get => _rulerInfo;
            set => SetProperty(ref _rulerInfo, value);
        }
    }
}

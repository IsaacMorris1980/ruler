using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

using Ruler.Shared.Models;

namespace Ruler.Shared.Interfaces
{
    public interface IMainViewModel
    {
        RulerInfo RulerData { get; }
        event EventHandler CloseRequested;
        void RequestClose();
    }
}

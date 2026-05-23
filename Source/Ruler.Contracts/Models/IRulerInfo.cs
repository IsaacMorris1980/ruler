using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ruler.Contracts.Enums;

namespace Ruler.Contracts.Models
{
    public interface IRulerInfo
    {
        int Width { get; set; }
        int Height { get; set; }
        bool IsVertical { get; set; }
        double Opacity { get; set; }
        bool ShowToolTip { get; set; }
        bool IsLocked { get; set; }
        bool TopMost { get; set; }
        int Top { get; set; }
        int Left { get; set; }
        SaveTypes SaveType { get; set; }
        string DisplayLocationString { get; set; }
        bool IsGuidelineEnabled { get; set; }
        double GuidelineLocation { get; set; }
    }
}

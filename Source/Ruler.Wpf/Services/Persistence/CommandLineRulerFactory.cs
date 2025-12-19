using Ruler.Wpf.Common;
using Ruler.Wpf.Models;

using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ruler.Wpf.Enums;

namespace Ruler.Wpf.Services.Persistence
{
    public static class CommandLineRulerFactory
    {       
        public static string ConvertToParameters(RulerInfo rulerInfo)
        {
            return string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9}", rulerInfo.Width, rulerInfo.Height, rulerInfo.IsVertical, rulerInfo.Opacity, rulerInfo.ShowToolTip, rulerInfo.IsLocked, rulerInfo.TopMost,rulerInfo.Left,rulerInfo.Top, rulerInfo.SaveType);
        }
        public static RulerInfo CovertToRulerInfo(string[] args)
        {
            string width = args[0];
            string height = args[1];
            string isVertical = args[2];
            string opacity = args[3];
            string showToolTip = args[4];
            string isLocked = args[5];
            string topMost = args[6];
            string left = (args.Length >= 8) ? args[7] : "0";
            string top = (args.Length >= 9) ? args[8] : "0";
            string savetype = (args.Length >= 10) ? args[9] : "none";

            SaveTypes saveArgs;
                  if (!Enum.TryParse<SaveTypes>(savetype, true, out saveArgs))
            {
                saveArgs = SaveTypes.none;
            }

            RulerInfo rulerInfo = new RulerInfo
            {
                Width = int.Parse(width),
                Height = int.Parse(height),
                IsVertical = bool.Parse(isVertical),
                Opacity = double.Parse(opacity),
                ShowToolTip = bool.Parse(showToolTip),
                IsLocked = bool.Parse(isLocked),
                TopMost = bool.Parse(topMost),
                Left = double.Parse(left),
                Top = double.Parse(top),
                SaveType = saveArgs
            };

            return rulerInfo;
        }
    }
}

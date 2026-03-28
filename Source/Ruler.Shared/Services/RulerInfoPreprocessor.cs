using Ruler.Wpf.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ruler.Shared.Interfaces;
using Ruler.Shared.Models;

namespace Ruler.Shared.Services
{
    public class RulerInfoPreprocessor : IDataPreprocessor<RulerInfo>
    {
        public IEnumerable<RulerInfo> Preprocess(IEnumerable<RulerInfo> items)
        {
            return items
                .Where(x => x.SaveType != Enums.SaveTypes.none)
                .Select(PrepareForSave);
        }

        private RulerInfo PrepareForSave(RulerInfo item)
        {
            if (item == null) return null;

            RulerInfo strippedCopy = RulerFactory.CreateDefault();

            // Ensure the SaveType carries over
            strippedCopy.SaveType = item.SaveType;

            switch (item.SaveType)
            {
                case Enums.SaveTypes.all:
                    RulerFactory.CopyValues(item, strippedCopy);
                    break;

                case Enums.SaveTypes.location:
                    strippedCopy.Left = item.Left;
                    strippedCopy.Top = item.Top;
                    strippedCopy.IsVertical = item.IsVertical;
                    break;

                case Enums.SaveTypes.size:
                    strippedCopy.Width = item.Width;
                    strippedCopy.Height = item.Height;
                    strippedCopy.IsVertical = item.IsVertical;
                    break;

                case Enums.SaveTypes.appearance:
                    strippedCopy.Opacity = item.Opacity;
                    strippedCopy.ShowToolTip = item.ShowToolTip;
                    strippedCopy.IsLocked = item.IsLocked;
                    strippedCopy.TopMost = item.TopMost;
                    break;
            }

            return strippedCopy;
        }
    }
}

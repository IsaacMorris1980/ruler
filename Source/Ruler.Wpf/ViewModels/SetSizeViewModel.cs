using Ruler.Wpf.Infrastructure;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Wpf.ViewModels
{
    public class SetSizeViewModel : ViewModelBase
    {
        private int _width;
        private int _height;

        public SetSizeViewModel(int currentWidth, int currentHeight)
        {
            Width = currentWidth;
            Height = currentHeight;
        }

        public int Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        public int Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ruler.Contracts.Strategies;

namespace Ruler.Shared.ViewModels
{
    public class SetSizeViewModel : ViewModelBase
    {
        private IUnitStrategy _selectedStrategy;
       
        public double WidthInput { get; set; }
        public double HeightInput { get; set; }

        // Expose all available units to the view's ComboBox
        public List<IUnitStrategy> AvailableStrategies { get; }

        public IUnitStrategy SelectedStrategy
        {
            get => _selectedStrategy;
            set
            {
                if (_selectedStrategy != value)
                {
                    // Advanced detail: If you want to convert the numbers inside the textboxes 
                    // on the fly when they switch units, you would run the math here!
                  SetProperty(ref _selectedStrategy, value);
                 
                }
            }
        }

        public SetSizeViewModel(double width, double height, IUnitStrategy active, IEnumerable<IUnitStrategy> allStrategies)
        {
            WidthInput = width;
            HeightInput = height;
            AvailableStrategies = allStrategies.ToList();
            _selectedStrategy = active;
        }
    }
}

using Ruler.Wpf.Enums;
using Ruler.Wpf.Models;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Ruler.Shared.Enums;
using Ruler.Shared.Models;
using Ruler.Shared.Interfaces;

namespace Ruler.Shared.Services
{
    public class OptionService : IOptionService
    {
        public ObservableCollection<UnitOption> Units { get; } = new ObservableCollection<UnitOption>();
        public ObservableCollection<OpacityOption> Opacities { get; } = new ObservableCollection<OpacityOption>();
        public ObservableCollection<ScaleOption> Scales { get; } = new ObservableCollection<ScaleOption>();
        public ObservableCollection<SaveOption> SaveOptions { get; } = new ObservableCollection<SaveOption>();

        public OptionService()
        {
            // Initialize once
            Units = new ObservableCollection<UnitOption>
        {
            new UnitOption(MeasurementUnit.Pixels),
            new UnitOption(MeasurementUnit.Points),
            new UnitOption(MeasurementUnit.Inches),
            new UnitOption(MeasurementUnit.Centimeters),
            new UnitOption(MeasurementUnit.Millimeters),
        };

            for (double opactiy = 0.5; opactiy <= 1.0; opactiy += 0.5)
            {
                Opacities.Add(new OpacityOption(opactiy));
            }
            double[] manualScales = new double[]
         {
                0.0,0.25,0.33,0.50,0.67,0.75,0.80,0.90,1.00,1.10,1.25,1.50,1.75,2.00,2.50,3.00,4.00,5.00
         };
            foreach (var scale in manualScales)
            {
                Scales.Add(new ScaleOption(scale));
            }
            SaveOptions = new ObservableCollection<SaveOption>
            {
                new SaveOption(SaveTypes.all),
                new SaveOption(SaveTypes.appearance),
                new SaveOption(SaveTypes.location),
                new SaveOption(SaveTypes.size),
                new SaveOption(SaveTypes.none)
            };

        }
    }
}

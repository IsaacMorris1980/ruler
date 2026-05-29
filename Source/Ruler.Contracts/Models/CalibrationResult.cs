using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruler.Contracts.Models
{
    public struct CalibrationResult
    {
        public double PixelsPerMm { get; }

        public CalibrationResult(double pixelsPerMm)
        {
            PixelsPerMm = pixelsPerMm;
        }
    }
}

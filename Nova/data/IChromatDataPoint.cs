using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Data
{
  internal interface IChromatDataPoint
  {
    double RT { get; set; } //retention time, unit defined elsewhere
    double Intensity { get; set; }
  }

  public struct ChromatDataPoint : IChromatDataPoint
  {
    public double RT { get; set; }
    public double Intensity { get; set; }
  }
}

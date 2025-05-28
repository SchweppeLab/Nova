using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Data
{
  internal interface IChromatDataPoint
  {
    double RT { get; set; } //retention time, unit defined elsewhere
    double Intensity { get; set; }
    void Read(BinaryReader reader);
    void Write(BinaryWriter writer);
  }

  public struct ChromatDataPoint : IChromatDataPoint
  {
    public double RT { get; set; }
    public double Intensity { get; set; }

    public void Read(BinaryReader reader)
    {
      RT = reader.ReadDouble();
      Intensity = reader.ReadDouble();
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(RT);
      writer.Write(Intensity);
    }
  }
}

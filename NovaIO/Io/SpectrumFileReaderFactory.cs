using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Io
{
  public class SpectrumFileReaderFactory
  {
    public static ISpectrumFileReader GetReader(string file, MSFilter filter)
    {
      string extension = Path.GetExtension(file).ToUpper();
      if (extension == ".MZXML")
      {
        throw new ArgumentException("Unsupported file extension: " + extension);
      }
      else if (extension == ".RAW")
      {
        ThermoRawReader r = new ThermoRawReader(filter);
        r.Open(file);
        return r;
      }
      else if (extension == ".MZDB")
      {
        throw new ArgumentException("Unsupported file extension: " + extension);
      }
      else if (extension == ".MZML")
      {
        MzMLReader r = new MzMLReader(filter);
        r.Open(file);
        return r;
      }
      throw new ArgumentException("Unrecognized file extension: " + extension);
    }
  }
}

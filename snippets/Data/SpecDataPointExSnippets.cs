// Snippets published on the SpecDataPointEx page.

using System;
using Nova.Data;
using Nova.Io.Read;

namespace NovaSnippets.Data
{
  internal static class SpecDataPointExSnippets
  {
    internal static void ReadExtendedPeaks()
    {
      FileReader reader = new FileReader("DDA.raw", MSFilter.MS2);
      SpectrumEx spectrum = reader.ReadSpectrumEx("", 1000);

      for (int i = 0; i < spectrum.Count; i++)
      {
        SpecDataPointEx peak = spectrum.DataPoints[i];
        Console.WriteLine(peak.Mz + "\t" + peak.Intensity + "\tz=" + peak.Charge
          + "\tS/N=" + peak.Intensity / peak.Noise);
      }
    }
  }
}

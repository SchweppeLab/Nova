// Snippets published on the TSpectrum, Spectrum and SpectrumEx pages.

using System;
using Nova.Data;
using Nova.Io.Read;

namespace NovaSnippets.Data
{
  internal static class SpectrumSnippets
  {
    // TSpectrum<T>: Resize, Serialize, Deserialize.
    internal static void RoundTrip()
    {
      Spectrum spectrum = new Spectrum();

      // Assigning DataPoints directly does not update Count, so size with Resize.
      spectrum.Resize(2);
      spectrum.DataPoints[0] = new SpecDataPoint(445.12, 10523.0);
      spectrum.DataPoints[1] = new SpecDataPoint(522.77, 850.0);

      byte[] payload = spectrum.Serialize();

      Spectrum copy = new Spectrum();
      copy.Deserialize(payload);
      Console.WriteLine(copy.Count);
    }

    // Spectrum
    internal static void CountScans()
    {
      int ms1Count = 0;
      int ms2Count = 0;
      FileReader reader = new FileReader("TheBestDataEver.mzML", MSFilter.MS1 | MSFilter.MS2);
      foreach (Spectrum spec in reader)
      {
        if (spec.MsLevel == 1)
        {
          ms1Count++;
        }
        else
        {
          ms2Count++;
        }
      }
      Console.WriteLine("There are " + ms1Count + " MS scans and " + ms2Count + " MS/MS scans.");
    }

    // SpectrumEx
    internal static void CountChargedPeaks()
    {
      FileReader reader = new FileReader("DDA.raw", MSFilter.MS1);
      SpectrumEx spectrum = reader.ReadSpectrumEx("", 1);

      int charged = 0;
      for (int i = 0; i < spectrum.Count; i++)
      {
        if (spectrum.DataPoints[i].Charge > 0) charged++;
      }
      Console.WriteLine(charged + " of " + spectrum.Count + " peaks have an assigned charge.");
    }
  }
}

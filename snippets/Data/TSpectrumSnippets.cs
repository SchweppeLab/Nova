// Snippets published on the TSpectrum method pages.
// One method here per page. Keep the bodies in step with the pages.

using System;
using Nova.Data;
using Nova.Io.Read;

namespace NovaSnippets.Data
{
  internal static class TSpectrumSnippets
  {
    internal static void GetMz()
    {
      Spectrum spectrum = new Spectrum(3);
      spectrum.DataPoints[0] = new SpecDataPoint(445.1200, 10523.0);
      spectrum.DataPoints[1] = new SpecDataPoint(522.7700, 850.0);
      spectrum.DataPoints[2] = new SpecDataPoint(600.3000, 2000.0);

      int exact = spectrum.GetMz(522.7700);       // 1
      int near = spectrum.GetMz(522.7720, 10);    // 1: 3.8 ppm away
      int miss = spectrum.GetMz(522.7800, 10);    // -1: 19 ppm away
    }

    internal static void Resize()
    {
      Spectrum spectrum = new Spectrum();

      // Assigning DataPoints directly does not update Count, so size with Resize.
      spectrum.Resize(2);
      spectrum.DataPoints[0] = new SpecDataPoint(445.12, 10523.0);
      spectrum.DataPoints[1] = new SpecDataPoint(522.77, 850.0);
      Console.WriteLine(spectrum.Count);          // 2
    }

    internal static void Serialize()
    {
      FileReader reader = new FileReader("DDA.raw", MSFilter.MS2);
      Spectrum spectrum = reader.ReadSpectrum();

      byte[] payload = spectrum.Serialize();      // store it, or send it in a PipeMessage
    }

    internal static void Deserialize(byte[] payload)
    {
      Spectrum copy = new Spectrum();
      copy.Deserialize(payload);                  // payload from Serialize
      Console.WriteLine(copy.ScanNumber + ": " + copy.Count + " points");
    }

    internal static void Dispose()
    {
      using (Spectrum spectrum = new Spectrum(100))
      {
        Console.WriteLine(spectrum.Count);
      }
    }
  }
}

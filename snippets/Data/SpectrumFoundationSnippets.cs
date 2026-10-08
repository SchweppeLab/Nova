// Snippets published on the SpectrumFoundation, ISpectrum, PrecursorIon and
// FragmentationType pages.

using System;
using Nova.Data;
using Nova.Io.Read;

namespace NovaSnippets.Data
{
  internal static class SpectrumFoundationSnippets
  {
    // SpectrumFoundation: the scan-level fields every spectrum type carries.
    internal static void ReadScanFields()
    {
      FileReader reader = new FileReader("DDA.raw", MSFilter.MS2);
      foreach (Spectrum spectrum in reader)
      {
        Console.WriteLine(spectrum.ScanNumber + "\t" + spectrum.RetentionTime
          + "\tMS" + spectrum.MsLevel + "\t" + spectrum.TotalIonCurrent);
      }
    }

    // PrecursorIon and FragmentationType, read from a dependent scan.
    internal static void ReadPrecursors()
    {
      FileReader reader = new FileReader("DDA.raw", MSFilter.MS2);
      foreach (Spectrum spectrum in reader)
      {
        foreach (PrecursorIon precursor in spectrum.Precursors)
        {
          Console.WriteLine(precursor.MonoisotopicMz + "\t" + precursor.Charge
            + "\t" + precursor.FragmentationMethod);

          if (precursor.FragmentationMethod == FragmentationType.HCD)
          {
            Console.WriteLine("HCD at " + precursor.CollisionEnergy);
          }
        }
      }
    }

    // PrecursorIon constructor.
    internal static void BuildAPrecursor()
    {
      PrecursorIon precursor = new PrecursorIon(652.3412, 1.2e6, 2, 652.84, 1.6);
      precursor.FragmentationMethod = FragmentationType.HCD;

      // The copy constructor carries every field.
      PrecursorIon copy = new PrecursorIon(precursor);
      Console.WriteLine(copy.IsolationWidth);
    }

    // ISpectrum<T>.GetMz
    internal static void FindAPeak(Spectrum spectrum)
    {
      // Nearest point within 10 ppm of the target, or -1.
      int index = spectrum.GetMz(445.1200, 10);
      if (index >= 0)
      {
        Console.WriteLine(spectrum.DataPoints[index].Intensity);
      }
    }

    // Working through the interface, for code that handles both spectrum types.
    internal static double TotalIntensity<T>(ISpectrum<T> spectrum) where T : struct, ISpecDataPoint
    {
      double total = 0;
      for (int i = 0; i < spectrum.Count; i++)
      {
        total += spectrum.DataPoints[i].Intensity;
      }
      return total;
    }
  }
}

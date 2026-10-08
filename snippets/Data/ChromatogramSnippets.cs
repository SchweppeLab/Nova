// Snippets published on the Chromatogram, IChromatogram, ChromatDataPoint
// and IChromatDataPoint pages.

using System;
using Nova.Data;
using Nova.Io.Read;

namespace NovaSnippets.Data
{
  internal static class ChromatogramSnippets
  {
    internal static void ReadChromatogramsFromAFile()
    {
      FileReader reader = new FileReader("DDA.mzML");

      for (int i = 0; i < reader.ChromatCount; i++)
      {
        Chromatogram chromat = reader.ReadChromatogram("", i);
        Console.WriteLine(chromat.ID + ": " + chromat.Count + " points");

        for (int j = 0; j < chromat.Count; j++)
        {
          ChromatDataPoint point = chromat.DataPoints[j];
          Console.WriteLine(point.RT + "\t" + point.Intensity);
        }
      }
    }

    internal static void BuildAChromatogram()
    {
      Chromatogram chromat = new Chromatogram();

      // Assigning DataPoints directly does not update Count, so size with Resize.
      chromat.Resize(3);
      chromat.DataPoints[0] = new ChromatDataPoint { RT = 10.0, Intensity = 1200 };
      chromat.DataPoints[1] = new ChromatDataPoint { RT = 10.5, Intensity = 5400 };
      chromat.DataPoints[2] = new ChromatDataPoint { RT = 11.0, Intensity = 2100 };

      Console.WriteLine(chromat.Count);
    }

    internal static void RoundTrip()
    {
      Chromatogram chromat = new Chromatogram(2);
      chromat.DataPoints[0] = new ChromatDataPoint { RT = 10.0, Intensity = 1200 };
      chromat.DataPoints[1] = new ChromatDataPoint { RT = 10.5, Intensity = 5400 };

      byte[] payload = chromat.Serialize();

      Chromatogram copy = new Chromatogram();
      copy.Deserialize(payload);

      // ID is not carried by Serialize.
      Console.WriteLine(copy.Count);
    }
  }
}

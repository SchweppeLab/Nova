// Snippets published on the SpecDataPoint method pages.
// One method here per documented method. Keep the names in step with the pages.

using System;
using System.IO;
using Nova.Data;

namespace NovaSnippets.Data
{
  internal static class SpecDataPointSnippets
  {
    // SpecDataPoint(double mz = 0, double intensity = 0)
    internal static void Constructor()
    {
      SpecDataPoint peak = new SpecDataPoint(445.12, 10523.0);

      // Both parameters default to zero, so this is a valid empty point.
      SpecDataPoint empty = new SpecDataPoint();

      Console.WriteLine(peak.Mz + " at " + peak.Intensity);
      Console.WriteLine(empty.Mz + " at " + empty.Intensity);
    }

    // Write(BinaryWriter writer)
    internal static byte[] Write()
    {
      SpecDataPoint peak = new SpecDataPoint(445.12, 10523.0);

      using (MemoryStream stream = new MemoryStream())
      using (BinaryWriter writer = new BinaryWriter(stream))
      {
        peak.Write(writer);

        // 16 bytes: Mz then Intensity, each a little-endian double.
        return stream.ToArray();
      }
    }

    // Read(BinaryReader reader)
    internal static SpecDataPoint Read(byte[] bytes)
    {
      SpecDataPoint peak = new SpecDataPoint();

      using (MemoryStream stream = new MemoryStream(bytes))
      using (BinaryReader reader = new BinaryReader(stream))
      {
        // Reads in the same order Write produced, from the current position.
        peak.Read(reader);
      }

      return peak;
    }

    // Round trip, showing that Read and Write agree on the layout.
    internal static void RoundTrip()
    {
      byte[] bytes = Write();
      SpecDataPoint peak = Read(bytes);

      Console.WriteLine(peak.Mz + " at " + peak.Intensity);
    }

    // CompareTo(SpecDataPoint x)
    internal static void SortByMz()
    {
      SpecDataPoint[] peaks =
      {
        new SpecDataPoint(600.30, 2000.0),
        new SpecDataPoint(445.12, 10523.0),
        new SpecDataPoint(522.77, 850.0)
      };

      // IComparable orders by Mz, so no comparer is needed.
      Array.Sort(peaks);

      foreach (SpecDataPoint peak in peaks)
      {
        Console.WriteLine(peak.Mz);
      }
    }
  }
}

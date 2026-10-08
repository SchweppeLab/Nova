// Generated from the Example sections of the method pages. Do not hand-edit;
// change the page and regenerate.

using Nova.Data;
using System.IO;
using System;

namespace NovaSnippets.Data
{
  internal static class DataMethodSnippets
  {
    internal static void Chromatogram_Constructor()
    {
      Chromatogram chromat = new Chromatogram(2);
      chromat.ID = "TIC";
      chromat.DataPoints[0] = new ChromatDataPoint { RT = 10.0, Intensity = 1200 };
      chromat.DataPoints[1] = new ChromatDataPoint { RT = 10.5, Intensity = 5400 };
    }

    internal static void Chromatogram_Deserialize()
    {
      Chromatogram chromat = new Chromatogram(1);
      chromat.DataPoints[0] = new ChromatDataPoint { RT = 10.0, Intensity = 1200 };
      byte[] payload = chromat.Serialize();

      Chromatogram copy = new Chromatogram();
      copy.Deserialize(payload);
      Console.WriteLine(copy.Count);   // 1
    }

    internal static void Chromatogram_Dispose()
    {
      using (Chromatogram chromat = new Chromatogram(100))
      {
        Console.WriteLine(chromat.Count);
      }
    }

    internal static void Chromatogram_Resize()
    {
      Chromatogram chromat = new Chromatogram();
      chromat.Resize(3);
      chromat.DataPoints[0] = new ChromatDataPoint { RT = 10.0, Intensity = 1200 };
      chromat.DataPoints[1] = new ChromatDataPoint { RT = 10.5, Intensity = 5400 };
      chromat.DataPoints[2] = new ChromatDataPoint { RT = 11.0, Intensity = 2100 };
      Console.WriteLine(chromat.Count);   // 3
    }

    internal static void Chromatogram_Serialize()
    {
      Chromatogram chromat = new Chromatogram(2);
      chromat.DataPoints[0] = new ChromatDataPoint { RT = 10.0, Intensity = 1200 };
      chromat.DataPoints[1] = new ChromatDataPoint { RT = 10.5, Intensity = 5400 };

      byte[] payload = chromat.Serialize();   // store it, or send it in a PipeMessage
    }

    internal static void ChromatDataPoint_Read()
    {
      byte[] bytes;
      using (MemoryStream stream = new MemoryStream())
      using (BinaryWriter writer = new BinaryWriter(stream))
      {
        new ChromatDataPoint { RT = 10.5, Intensity = 5400 }.Write(writer);
        bytes = stream.ToArray();
      }

      ChromatDataPoint point = new ChromatDataPoint();
      using (BinaryReader reader = new BinaryReader(new MemoryStream(bytes)))
      {
        point.Read(reader);
      }
    }

    internal static void ChromatDataPoint_Write()
    {
      ChromatDataPoint point = new ChromatDataPoint { RT = 10.5, Intensity = 5400 };

      using (MemoryStream stream = new MemoryStream())
      using (BinaryWriter writer = new BinaryWriter(stream))
      {
        point.Write(writer);
        byte[] bytes = stream.ToArray();   // 16 bytes
      }
    }

    internal static void PrecursorIon_Clear()
    {
      PrecursorIon precursor = new PrecursorIon(652.3412, 1.2e6, 2);
      precursor.Clear();
      Console.WriteLine(precursor.MonoisotopicMz);   // 0
    }

    internal static void PrecursorIon_Constructor()
    {
      PrecursorIon precursor = new PrecursorIon(652.3412, 1.2e6, 2, 652.84, 1.6);
      precursor.FragmentationMethod = FragmentationType.HCD;

      // The copy carries every field.
      PrecursorIon copy = new PrecursorIon(precursor);
    }

    internal static void SpecDataPointEx_CompareTo()
    {
      SpecDataPointEx[] peaks =
      {
        new SpecDataPointEx(600.30, 2000.0),
        new SpecDataPointEx(445.12, 10523.0),
        new SpecDataPointEx(522.77, 850.0)
      };

      Array.Sort(peaks);
    }

    internal static void SpecDataPointEx_Constructor()
    {
      SpecDataPointEx peak = new SpecDataPointEx(445.12, 10523.0, 150.0, 0, 2, 120000);

      // Named arguments set only what is needed.
      SpecDataPointEx simple = new SpecDataPointEx(mz: 445.12, intensity: 10523.0);
    }

    internal static void SpecDataPointEx_Read()
    {
      byte[] bytes;
      using (MemoryStream stream = new MemoryStream())
      using (BinaryWriter writer = new BinaryWriter(stream))
      {
        new SpecDataPointEx(445.12, 10523.0, 150.0, 0, 2, 120000).Write(writer);
        bytes = stream.ToArray();
      }

      SpecDataPointEx peak = new SpecDataPointEx();
      using (BinaryReader reader = new BinaryReader(new MemoryStream(bytes)))
      {
        peak.Read(reader);
      }
    }

    internal static void SpecDataPointEx_Write()
    {
      SpecDataPointEx peak = new SpecDataPointEx(445.12, 10523.0, 150.0, 0, 2, 120000);

      using (MemoryStream stream = new MemoryStream())
      using (BinaryWriter writer = new BinaryWriter(stream))
      {
        peak.Write(writer);
        byte[] bytes = stream.ToArray();   // 44 bytes
      }
    }

    internal static void Spectrum_Constructor()
    {
      Spectrum spectrum = new Spectrum(2);
      spectrum.ScanNumber = 1;
      spectrum.MsLevel = 1;
      spectrum.DataPoints[0] = new SpecDataPoint(445.12, 10523.0);
      spectrum.DataPoints[1] = new SpecDataPoint(522.77, 850.0);
    }

    internal static void SpectrumEx_Constructor()
    {
      SpectrumEx spectrum = new SpectrumEx(1);
      spectrum.DataPoints[0] = new SpecDataPointEx(445.12, 10523.0, 150.0, 0, 2, 120000);
    }
  }
}

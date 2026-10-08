// Snippets published on the FileReader, FileFormat, MSFilter and SpectrumFileReaderFactory pages.

using System;
using Nova.Data;
using Nova.Io.Read;

namespace NovaSnippets.IoRead
{
  internal static class ReaderBasicsSnippets
  {
    // FileReader
    internal static void ReadMs1()
    {
      // Reads all MS1 scans from a Thermo Fisher Scientific data file
      FileReader reader = new FileReader("DDA.raw", MSFilter.MS1);
      foreach (Spectrum spec in reader)
      {
        // Report each scan number and number of data points
        Console.WriteLine(spec.ScanNumber + " has " + spec.Count + " data points.");
      }
    }

    // FileFormat
    internal static void CheckFormat()
    {
      FileFormat format = FileReader.CheckFileFormat("DDA.mzML");
      if (format == FileFormat.MzML)
      {
        Console.WriteLine("An mzML file.");
      }
    }

    // MSFilter
    internal static void FilterLevels()
    {
      // Read MS1 and MS2 spectra, skipping MS3.
      FileReader reader = new FileReader("DDA.raw", MSFilter.MS1 | MSFilter.MS2);
      foreach (Spectrum spectrum in reader)
      {
        Console.WriteLine(spectrum.ScanNumber + "\tMS" + spectrum.MsLevel);
      }
    }

    // SpectrumFileReaderFactory
    internal static void UseTheFactory()
    {
      ISpectrumFileReader reader = SpectrumFileReaderFactory.GetReader("DDA.mzML", MSFilter.MS2);
      Console.WriteLine(reader.ScanCount + " spectra, scans " + reader.FirstScan + " to " + reader.LastScan);

      Spectrum spectrum = reader.GetSpectrum();   // the first MS2 spectrum
      Console.WriteLine(spectrum.Count + " peaks");

      reader.Close();
    }
  }
}

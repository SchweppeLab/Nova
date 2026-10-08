// Generated from the Example sections of the method pages. Do not hand-edit;
// change the page and regenerate.

using Nova.Data;
using Nova.Io.Read;
using System;

namespace NovaSnippets.IoRead
{
  internal static class IoReadMethodSnippets
  {
    internal static void FileReader_CheckFileFormat()
    {
      FileFormat format = FileReader.CheckFileFormat("DDA.RAW");   // FileFormat.ThermoRaw
    }

    internal static void FileReader_Constructor()
    {
      // Open a file now, reading only MS2 spectra.
      FileReader reader = new FileReader("DDA.mzML", MSFilter.MS2);

      // Or create the reader first and name the file when reading.
      FileReader later = new FileReader();
      later.ReadSpectrum("DDA.mzML", 1000);
    }

    internal static void FileReader_GetEnumerator()
    {
      FileReader reader = new FileReader("DDA.raw", MSFilter.MS2);
      foreach (Spectrum spectrum in reader)
      {
        Console.WriteLine(spectrum.ScanNumber + "\t" + spectrum.RetentionTime);
      }
    }

    internal static void FileReader_OpenSpectrumFile()
    {
      FileReader reader = new FileReader();
      if (reader.OpenSpectrumFile("DDA.mzML"))
      {
        Console.WriteLine(reader.ScanCount + " spectra, scans " + reader.FirstScan + " to " + reader.LastScan);
      }
    }

    internal static void FileReader_ReadChromatogram()
    {
      FileReader reader = new FileReader("DDA.mzML");
      for (int i = 0; i < reader.ChromatCount; i++)
      {
        Chromatogram chromat = reader.ReadChromatogram("", i);
        Console.WriteLine(chromat.ID + ": " + chromat.Count + " points");
      }
    }

    internal static void FileReader_ReadSpectrum()
    {
      FileReader reader = new FileReader(MSFilter.MS1);

      // The first call names the file; later calls keep reading it.
      Spectrum spectrum = reader.ReadSpectrum("DDA.raw");
      while (spectrum.ScanNumber > 0)
      {
        Console.WriteLine(spectrum.ScanNumber + ": " + spectrum.Count + " peaks");
        spectrum = reader.ReadSpectrum();
      }
    }

    internal static void FileReader_ReadSpectrumEx()
    {
      FileReader reader = new FileReader(MSFilter.MS1);

      // The first call names the file; later calls keep reading it.
      SpectrumEx spectrum = reader.ReadSpectrumEx("DDA.raw");
      while (spectrum.ScanNumber > 0)
      {
        Console.WriteLine(spectrum.ScanNumber + ": " + spectrum.Count + " peaks");
        spectrum = reader.ReadSpectrumEx();
      }
    }

    internal static void FileReader_Reset()
    {
      FileReader reader = new FileReader("DDA.raw");
      Spectrum first = reader.ReadSpectrum();
      Spectrum second = reader.ReadSpectrum();

      reader.Reset();
      Spectrum again = reader.ReadSpectrum();   // the same scan as first
    }

    internal static void FileReader_SetFilter()
    {
      FileReader reader = new FileReader("DDA.raw", MSFilter.MS1);
      int ms1 = 0;
      foreach (Spectrum spectrum in reader) ms1++;

      reader.SetFilter(MSFilter.MS2 | MSFilter.MS3);
      int msn = 0;
      foreach (Spectrum spectrum in reader) msn++;
    }

    internal static void ISpectrumFileReader_Close()
    {
      ISpectrumFileReader reader = SpectrumFileReaderFactory.GetReader("DDA.mzML", MSFilter.MS2);
      Spectrum spectrum = reader.GetSpectrum();
      reader.Close();
    }

    internal static void ISpectrumFileReader_GetChromatogram()
    {
      ISpectrumFileReader reader = SpectrumFileReaderFactory.GetReader("DDA.mzML", MSFilter.MS1);
      for (int i = 0; i < reader.ChromatCount; i++)
      {
        Chromatogram chromat = reader.GetChromatogram(i);
        Console.WriteLine(chromat.ID + ": " + chromat.Count + " points");
      }
      reader.Close();
    }

    internal static void ISpectrumFileReader_GetSpectrum()
    {
      ISpectrumFileReader reader = SpectrumFileReaderFactory.GetReader("DDA.mzML", MSFilter.MS2);
      Spectrum spectrum = reader.GetSpectrum();
      while (spectrum.ScanNumber > 0)
      {
        Console.WriteLine(spectrum.ScanNumber + ": " + spectrum.Count + " peaks");
        spectrum = reader.GetSpectrum();
      }
      reader.Close();
    }

    internal static void ISpectrumFileReader_GetSpectrumEx()
    {
      ISpectrumFileReader reader = SpectrumFileReaderFactory.GetReader("DDA.raw", MSFilter.MS2);
      SpectrumEx spectrum = reader.GetSpectrumEx(1000);
      for (int i = 0; i < spectrum.Count; i++)
      {
        Console.WriteLine(spectrum.DataPoints[i].Mz + " at resolution " + spectrum.DataPoints[i].Resolution);
      }
      reader.Close();
    }

    internal static void ISpectrumFileReader_Open()
    {
      ISpectrumFileReader reader = SpectrumFileReaderFactory.GetReader("Run1.mzML", MSFilter.MS2);
      Console.WriteLine("Run1: " + reader.ScanCount + " spectra");

      if (reader.Open("Run2.mzML"))
      {
        Console.WriteLine("Run2: " + reader.ScanCount + " spectra");
      }
      reader.Close();
    }

    internal static void ISpectrumFileReader_Reset()
    {
      ISpectrumFileReader reader = SpectrumFileReaderFactory.GetReader("DDA.mzML", MSFilter.MS2);
      Spectrum first = reader.GetSpectrum();

      reader.Reset();
      Spectrum again = reader.GetSpectrum();   // the same scan as first
      reader.Close();
    }

    internal static void SpectrumFileReaderFactory_GetReader()
    {
      try
      {
        ISpectrumFileReader reader = SpectrumFileReaderFactory.GetReader("DDA.mzML", MSFilter.MS2);
        Spectrum spectrum = reader.GetSpectrum();
        reader.Close();
      }
      catch (SpectrumFileOpenException ex)
      {
        Console.WriteLine(ex.Message);
      }
    }

    internal static void SpectrumFileOpenException_Constructor()
    {
      try
      {
        FileReader reader = new FileReader();
        reader.ReadSpectrum("Damaged.mzML");
      }
      catch (SpectrumFileOpenException ex)
      {
        Console.WriteLine(ex.FileName + ": " + ex.Message);
      }
    }
  }
}

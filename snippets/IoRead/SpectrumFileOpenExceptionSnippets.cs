// Snippets published on the SpectrumFileOpenException page.

using System;
using System.IO;
using Nova.Data;
using Nova.Io.Read;

namespace NovaSnippets.IoRead
{
  internal static class SpectrumFileOpenExceptionSnippets
  {
    internal static void CatchAFailedOpen()
    {
      FileReader reader = new FileReader();

      try
      {
        Spectrum spectrum = reader.ReadSpectrum("missing.mzML");
      }
      catch (SpectrumFileOpenException ex)
      {
        Console.WriteLine(ex.FileName + ": " + ex.Message);
      }
    }

    internal static void CatchAsIOException()
    {
      // Derives from IOException, so existing file-error handling still catches it.
      try
      {
        ISpectrumFileReader reader = SpectrumFileReaderFactory.GetReader("missing.mzML", MSFilter.MS1);
      }
      catch (IOException ex)
      {
        Console.WriteLine(ex.Message);
      }
    }
  }
}

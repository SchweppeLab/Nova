// Snippets published on the MzMLWriter pages.

using System;
using Nova.Data;
using Nova.Io.Read;
using Nova.Io.Write;

namespace NovaSnippets.IoWrite
{
  internal static class MzMLWriterSnippets
  {
    // The whole sequence: describe the file, start a run, add spectra, write.
    internal static void WriteAnMzMLFile()
    {
      MzMLWriter writer = new MzMLWriter();

      // Describe how the file was produced. Ids given here are referred to
      // by the entries that follow.
      writer.AddSoftware("Nova", "1.1.0");
      writer.AddInstrumentConfiguration("IC1", null);
      writer.AddDataProcessing("DP1");
      writer.AddProcessingMethod("Nova");

      // A run must be started before any spectrum is added.
      writer.AddRun("run1", "IC1");

      FileReader reader = new FileReader("DDA.raw", MSFilter.MS1 | MSFilter.MS2);
      foreach (Spectrum spectrum in reader)
      {
        writer.AddSpectrum(spectrum);
      }

      writer.Write("converted.mzML");
    }

    // Write(string, bool, string)
    internal static void WriteAndValidate()
    {
      MzMLWriter writer = new MzMLWriter();
      writer.AddRun("run1", "IC1");

      // Validation re-opens the written file and checks it against a local
      // copy of the mzML schema. It is off by default because it needs that
      // file, and the output is usable either way.
      writer.Write("converted.mzML", true, @"C:\schemas\mzML1.1.0.xsd");
    }
  }
}

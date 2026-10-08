// Generated from the Example sections of the method pages. Do not hand-edit;
// change the page and regenerate.

using Nova.Data;
using Nova.Io.Meta;
using Nova.Io.Read;
using Nova.Io.Write;
using System;

namespace NovaSnippets.IoWrite
{
  internal static class IoWriteMethodSnippets
  {
    internal static void MzMLWriter_AddDataProcessing()
    {
      MzMLWriter writer = new MzMLWriter();
      writer.AddSoftware("Nova", "1.1.0");

      writer.AddDataProcessing("DP1");
      writer.AddProcessingMethod("Nova");
    }

    internal static void MzMLWriter_AddInstrumentConfiguration()
    {
      MzMLWriter writer = new MzMLWriter();
      writer.AddInstrumentConfiguration("IC1", null);
      writer.AddRun("run1", "IC1");
    }

    internal static void MzMLWriter_AddProcessingMethod()
    {
      MzMLWriter writer = new MzMLWriter();
      writer.AddSoftware("Xcalibur", "4.5");
      writer.AddSoftware("Nova", "1.1.0");

      // Methods are numbered in the order they are added.
      writer.AddDataProcessing("DP1");
      writer.AddProcessingMethod("Xcalibur");
      writer.AddProcessingMethod("Nova");
    }

    internal static void MzMLWriter_AddRun()
    {
      MzMLWriter writer = new MzMLWriter();
      writer.AddInstrumentConfiguration("IC1", null);
      writer.AddRun("run1", "IC1");

      FileReader reader = new FileReader("DDA.raw");
      foreach (Spectrum spectrum in reader)
      {
        writer.AddSpectrum(spectrum);
      }
    }

    internal static void MzMLWriter_AddSoftware()
    {
      MzMLWriter writer = new MzMLWriter();
      writer.AddSoftware("Xcalibur", "4.5");   // also gets its standard CV term
      writer.AddSoftware("Nova", "1.1.0");
    }

    internal static void MzMLWriter_AddSpectrum()
    {
      MzMLWriter writer = new MzMLWriter();
      writer.AddInstrumentConfiguration("IC1", null);
      writer.AddRun("run1", "IC1");

      // Keep only the MS2 spectra.
      FileReader reader = new FileReader("DDA.raw", MSFilter.MS2);
      foreach (Spectrum spectrum in reader)
      {
        writer.AddSpectrum(spectrum);
      }
      writer.Write("DDA_ms2.mzML");
    }

    internal static void MzMLWriter_Constructor()
    {
      MzMLWriter writer = new MzMLWriter();
    }

    internal static void MzMLWriter_Write()
    {
      MzMLWriter writer = new MzMLWriter();
      // ... add software, instrument configuration, a run and spectra ...

      writer.Write("converted.mzML");

      // Or write and validate against a local copy of the schema.
      writer.Write("converted.mzML", true, @"C:\schemas\mzML1.1.0.xsd");
    }

    internal static void MetaDictionary_FindMeta()
    {
      MetaClass a = MetaDictionary.FindMeta("Charge State");       // MetaClass.ChargeState
      MetaClass b = MetaDictionary.FindMeta("Z");                  // MetaClass.ChargeState
      MetaClass c = MetaDictionary.FindMeta("Not A Real Label");   // MetaClass.None
    }
  }
}

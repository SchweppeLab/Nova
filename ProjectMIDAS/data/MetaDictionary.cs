using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectMIDAS.data
{
  public enum MetaClass
  {
    None,
    ChargeState,
    Header,
    IIT,
    MonoisotopicMZ,
    ScanNumber,
    TIC,
    Trailer,
    Extra
  }

  public static class MetaDictionary
  {
    
    public static List<MetaItem> MetaItems { get; } = new List<MetaItem>()
    {
      // header items
      { new MetaItem("MassAnalyzer", "", new string[]{ "MassAnalyzer" }, MetaClass.Header) },
      { new MetaItem("ScanNumber", "", new string[]{ "Scan", "ScanNumber" }, MetaClass.ScanNumber) },
      { new MetaItem("FirstSpectrum", "", new string[]{ "FirstSpectrum" }, MetaClass.Header) },
      { new MetaItem("LastSpectrum", "", new string[]{ "LastSpectrum" }, MetaClass.Header) },
      { new MetaItem("StartTime", "", new string[]{ "StartTime" }, MetaClass.Header) },
      { new MetaItem("EndTime", "", new string[]{ "EndTime" }, MetaClass.Header) },
      { new MetaItem("MassResolution", "", new string[]{ "MassResolution" }, MetaClass.Header) },
      { new MetaItem("ExpectedRuntime", "", new string[]{ "ExpectedRuntime" }, MetaClass.Header) },
      { new MetaItem("MaxIntegratedIntensity", "", new string[]{ "MaxIntegratedIntensity" }, MetaClass.Header) },
      { new MetaItem("MaxIntensity", "", new string[]{ "MaxIntensity" }, MetaClass.Header) },
      { new MetaItem("ToleranceUnit", "", new string[]{ "ToleranceUnit" }, MetaClass.Header) },
      { new MetaItem("TIC", "", new string[]{ "TIC", "Total Ion Current" }, MetaClass.TIC) },
      { new MetaItem("BasePeakIntensity", "", new string[]{ "BasePeakIntensity" }, MetaClass.Header) },
      { new MetaItem("BasePeakMass", "", new string[]{ "BasePeakMass" }, MetaClass.Header) },
      { new MetaItem("InjectTime", "", new string[]{ "InjectTime", "Ion Injection Time (ms):" }, MetaClass.Header) },
      { new MetaItem("MasterScan", "", new string[]{ "MasterScan", "ParentScan", "Master Scan Number" }, MetaClass.Header) },
      { new MetaItem("PrecursorMass[0]", "", new string[]{ "PrecursorMass", "ParentMass" }, MetaClass.Header) },
      { new MetaItem("FirstMass", "", new string[]{ "FirstMass", "FirstMz", "LowMass" }, MetaClass.Header) },
      { new MetaItem("LastMass", "", new string[]{ "LastMass", "LastMz", "HighMass" }, MetaClass.Header) },
      { new MetaItem("ActivationType", "", new string[]{ "ActivationType" }, MetaClass.Header) },
      { new MetaItem("CollisionEnergy[0]", "", new string[]{ "CollisionEnergy", "NCE" }, MetaClass.Header) },
      { new MetaItem("MSOrder", "", new string[]{ "MSOrder", "ScanOrder" }, MetaClass.Header) },
      { new MetaItem("Dependent", "", new string[]{ "Dependent" }, MetaClass.Header) },
      { new MetaItem("MSX", "", new string[]{ "MSX" }, MetaClass.Header) },
      { new MetaItem("Resolution", "", new string[]{ "Resolution" }, MetaClass.Header) },
      { new MetaItem("RawOvFtT", "", new string[]{ "RawOvFtT" }, MetaClass.Header) },
      { new MetaItem("AGC", "", new string[]{ "AGC" }, MetaClass.Header) },
      { new MetaItem("ScanMode", "", new string[]{ "ScanMode" }, MetaClass.Header) },
      { new MetaItem("Filter", "", new string[]{ "Filter" }, MetaClass.Header) },
      { new MetaItem("ScanData", "", new string[]{ "ScanData" }, MetaClass.Header) },
      { new MetaItem("AccurateMass", "", new string[]{ "AccurateMass" }, MetaClass.Header) },
      { new MetaItem("Orbitrap Resolution", "", new string[]{ "Orbitrap Resolution" }, MetaClass.Header) },
      { new MetaItem("API Process Delay", "", new string[]{ "API Process Delay", "Process Delay" }, MetaClass.Header) },
      { new MetaItem("Isolation Width", "", new string[]{ "MS2 Isolation Width", "Isolation Width", "IsolationWidth" }, MetaClass.Header) },
      { new MetaItem("RetentionTime", "", new string[]{ "RetentionTime", "Retention Time", "RT", "StartTime" }, MetaClass.Header) },
      //trailer items
      { new MetaItem("Master Scan Number", "", new string[]{ "Master Scan Number", "MasterScan" }, MetaClass.Trailer) },
      { new MetaItem("Access ID", "", new string[]{ "Access ID", "AccessID" }, MetaClass.Trailer) },
      { new MetaItem("Elapsed Scan Time (sec)", "", new string[]{ "Elapsed Scan Time", "Elapsed Scan Time (sec)" }, MetaClass.Trailer) },
      { new MetaItem("Ion Injection Time (ms):", "", new string[]{ "Ion Injection Time (ms):" }, MetaClass.IIT) },
      { new MetaItem("Scan Description", "", new string[]{ "Scan Description", "ScanDesc" }, MetaClass.Trailer) },
      { new MetaItem("Monoisotopic M/Z", "", new string[]{ "Monoisotopic M/Z:", "Monoisotopic", "Mono M/Z" }, MetaClass.MonoisotopicMZ) },
      { new MetaItem("SPS Masses", "", new string[]{ "SPS Masses" }, MetaClass.Trailer) },
      { new MetaItem("SPS Masses Continued", "", new string[]{ "SPS Masses Continued" }, MetaClass.Trailer) },
      { new MetaItem("FAIMS Voltage On", "", new string[]{ "FAIMS Voltage On", "FAIMS Voltage On:" }, MetaClass.Trailer) },
      { new MetaItem("FAIMS Voltage", "", new string[]{ "FAIMS CV", "FAIMS Voltage" }, MetaClass.Trailer) },
      { new MetaItem("Charge State", "", new string[]{ "Charge State:", "Charge", "Z" }, MetaClass.ChargeState) },
    };

    public static MetaClass FindMeta(string label)
    {
      foreach (MetaItem item in MetaItems)
      {
        if (item.TryGetValue(label, out object outer))
        {
          return item.Class;
        }
      }
      return MetaClass.None;
    }
    
  }
}

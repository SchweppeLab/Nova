namespace Nova.Io.Meta
{
  public enum MetaClass
  {
    None,
    Analyzer,
    ChargeState,
    FaimsState,
    FaimsCV,
    //Header,
    IIT,
    MonoisotopicMZ,
    ScanNumber,
    TIC
    //Trailer,
    //Extra
  }

  public static class MetaDictionary
  {

    private static Dictionary<string, MetaClass> MetaTerms = new Dictionary<string, MetaClass>()
    {
      //Analyzer
      {"MassAnalyzer",MetaClass.Analyzer},

      //ChargeState
      {"Charge State",MetaClass.ChargeState },
      {"Charge State:",MetaClass.ChargeState },
      {"Charge",MetaClass.ChargeState },
      {"Z",MetaClass.ChargeState },

      //FaimsState
      {"FAIMS Voltage On",MetaClass.FaimsState },
      {"FAIMS Voltage On:", MetaClass.FaimsState },

      //FaimsCV
      {"FAIMS Voltage",MetaClass.FaimsCV },
      {"FAIMS CV",MetaClass.FaimsCV },

      //IIT
      {"Ion Injection Time (ms)",MetaClass.IIT },
      {"Ion Injection Time (ms):", MetaClass.IIT},

      //MonoisotopicMZ
      {"Monoisotopic M/Z", MetaClass.MonoisotopicMZ },
      {"Monoisotopic M/Z:", MetaClass.MonoisotopicMZ },
      {"Monoisotopic", MetaClass.MonoisotopicMZ },
      {"Mono M/Z", MetaClass.MonoisotopicMZ },

      //ScanNumber
      {"Scan", MetaClass.ScanNumber },
      {"ScanNumber", MetaClass.ScanNumber },

      //TIC
      {"TIC", MetaClass.TIC },
      {"Total Ion Current", MetaClass.TIC },


    };

    public static MetaClass FindMeta(string label)
    {
      if (MetaTerms.TryGetValue(label, out var metaClass))
      {
        return metaClass;
      }
      else
      {
        return MetaClass.None;
      }
    }    
  }
}

// Copyright 2025 Michael Hoopmann
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
    MasterScanNumber,
    MonoisotopicMZ,
    ScanDescription,
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

      //MasterScanNumber
      {"Master Scan Number",MetaClass.MasterScanNumber },
      {"Master Scan Number:",MetaClass.MasterScanNumber },

      //MonoisotopicMZ
      {"Monoisotopic M/Z", MetaClass.MonoisotopicMZ },
      {"Monoisotopic M/Z:", MetaClass.MonoisotopicMZ },
      {"Monoisotopic", MetaClass.MonoisotopicMZ },
      {"Mono M/Z", MetaClass.MonoisotopicMZ },

      //ScanDescription
      {"Scan Description", MetaClass.ScanDescription },
      {"Scan Description:", MetaClass.ScanDescription },

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

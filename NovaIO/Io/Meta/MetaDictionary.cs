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
  /// <summary>
  /// The kinds of scan metadata recognized in a Thermo trailer, by label
  /// </summary>
  public enum MetaClass
  {
    /// <summary>Not a recognized label</summary>
    None,
    /// <summary>Mass analyzer</summary>
    Analyzer,
    /// <summary>Precursor charge state</summary>
    ChargeState,
    /// <summary>Whether FAIMS was on</summary>
    FaimsState,
    /// <summary>FAIMS compensation voltage</summary>
    FaimsCV,
    //Header,
    /// <summary>Ion injection time</summary>
    IIT,
    /// <summary>The Thermo master index</summary>
    MasterIndex,
    /// <summary>Scan number of the master (parent) scan</summary>
    MasterScanNumber,
    /// <summary>Precursor monoisotopic m/z</summary>
    MonoisotopicMZ,
    /// <summary>Scan description</summary>
    ScanDescription,
    /// <summary>Scan number</summary>
    ScanNumber,
    /// <summary>Total ion current</summary>
    TIC
    //Trailer,
    //Extra
  }

  /// <summary>
  /// Maps a Thermo trailer label to the <see cref="MetaClass"/> it represents
  /// </summary>
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
      {"FAIMS CV:",MetaClass.FaimsCV },

      //IIT
      {"Ion Injection Time (ms)",MetaClass.IIT },
      {"Ion Injection Time (ms):", MetaClass.IIT},

      //MasterIndex
      {"Master Index",MetaClass.MasterIndex },
      {"Master Index:",MetaClass.MasterIndex },

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

    /// <summary>
    /// Looks up a trailer label
    /// </summary>
    /// <param name="label">The label as it appears in the file</param>
    /// <returns>The matching <see cref="MetaClass"/>, or <see cref="MetaClass.None"/> if the label is not recognized</returns>
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

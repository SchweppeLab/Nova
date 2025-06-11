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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Data
{
  public enum FramentationType
  {
    None,
    CID,
    ECD,
    ETD,
    EThcD,
    ETDSA,
    HCD,   
    IRMPD,
    PQD,
    SID
  }


  public abstract class SpectrumFoundation
  {

    /// <summary>
    /// SpectrumFoundation constructor.
    /// </summary>
    protected SpectrumFoundation() 
    {
      Precursors = new List<PrecursorIon>();
    }

    /// <summary>
    /// Identifies the peak data type as centroid (true) or profile (false)
    /// </summary>
    public bool Centroid { get; set; } = true;

    /// <summary>
    /// Total time, including injection time, to acquire the current scan (milliseconds)
    /// </summary>
    public double ElapsedScanTime { get; set; } = 0;

    /// <summary>
    /// Injection time used to acquire the scan ions (milliseconds, max = 5000)
    /// </summary>
    public double IonInjectionTime { get; set; } = 0;

    /// <summary>
    /// Generic key/value dictionary to contain whatever information might not be in a direct data member.
    /// </summary>
    public Dictionary<string, string> MetaData { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// The MS level (e.g. 0 = Unknown, 1 = MS1, 2 = MS2, 3 = MS3, MSn)
    /// </summary>
    public int MsLevel { get; set; } = 0;

    /// <summary>
    /// Bool representation of polarity (true = positive)
    /// </summary>
    public bool Polarity { get; set; } = true;

    /// <summary>
    /// The current scan description
    /// </summary>
    public string ScanDescription { get; set; } = string.Empty;

    /// <summary>
    /// Scan event order
    /// </summary>
    public int ScanEvent { get; set; } = 0;

    /// <summary>
    /// Current scan number
    /// </summary>
    public int ScanNumber { get; set; } = 0;

    /// <summary>
    /// Thermo variable for master scan number
    /// </summary>
    public int MasterIndex { get; set; } = 0;

    /// <summary>
    /// Mass analyzer used
    /// </summary>
    public string Analyzer { get; set; } = string.Empty;

    /// <summary>
    /// String description of scan type
    /// </summary>
    public string ScanType { get; set; } = string.Empty;

    /// <summary>
    /// Scan filter line from RAW file
    /// </summary>
    public string ScanFilter { get; set; } = string.Empty;

    /// <summary>
    /// Scan retention time (minutes)
    /// </summary>
    public double RetentionTime { get; set; } = 0;

    /// <summary>
    /// Mz that scan starts at
    /// </summary>
    public double StartMz { get; set; } = 0;

    /// <summary>
    /// Mz that scan ends at
    /// </summary>
    public double EndMz { get; set; } = 0;

    /// <summary>
    /// Lowest Mz observed in scan
    /// </summary>
    public double LowestMz { get; set; } = 0;

    /// <summary>
    /// Highest Mz observed in scan
    /// </summary>
    public double HighestMz { get; set; } = 0;

    /// <summary>
    /// The m/z of the most intense peak in the scan.
    /// </summary>
    public double BasePeakMz { get; set; } = 0;

    /// <summary>
    /// The abundance of the most intense peak in the scan.
    /// </summary>
    public double BasePeakIntensity { get; set; } = 0;

    /// <summary>
    /// FAIMS compensation voltage, if used (in volts)
    /// </summary>
    public double FaimsCV { get; set; } = 0;

    /// <summary>
    /// FAIMS state, true = on. FaimsState can be on, and FaimsCV can still be 0.
    /// </summary>
    public bool FaimsState { get; set; } = false;

    /// <summary>
    /// Total ion current for the current scan
    /// </summary>
    public double TotalIonCurrent { get; set; } = 0;

    /// <summary>
    /// Zero or more precursor ions associated with this spectrum.
    /// </summary>
    public List<PrecursorIon> Precursors { get; set; }

    /// <summary>
    /// If a dependent scan, the parent scan number
    /// </summary>
    public int PrecursorMasterScanNumber { get; set; } = 0;

    /// <summary>
    /// If a dependent scan, the activation method used to generate the scan fragments
    /// </summary>
    public string PrecursorActivationMethod { get; set; } = string.Empty;

    /// <summary>
    /// Detector or mass analyzer used for the scan.
    /// e.g. ITMS or FTMS
    /// </summary>
    public string DetectorType { get; set; } = string.Empty;

    /// <summary>
    /// "Scan Description" field from the scan header.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// This is the activation energy for a fragmentation event. I'm not sure if it belongs
    /// here or with the precursor ions. But it is here for now for compatibility with existing tools.
    /// </summary>
    public double CollisionEnergy { get; set; } = 0;
  }
}

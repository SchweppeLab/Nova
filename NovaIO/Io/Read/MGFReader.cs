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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Nova.Data;

namespace Nova.Io.Read
{
  /// <summary>
  /// Reads Mascot Generic Format (MGF) files, per the format described at
  /// https://www.matrixscience.com/help/data_file_help.html. MGF is a plain-text,
  /// line-oriented format with no built-in random-access index (unlike mzML/mzXML), and every
  /// spectrum in it is inherently an MS/MS (MsLevel 2) spectrum -- there is no MS1 concept.
  /// </summary>
  internal class MGFReader : ISpectrumFileReader
  {
    /// <summary>
    /// A basic spectrum type reading only mz and intensity values for each data point.
    /// </summary>
    private Spectrum spectrum;

    /// <summary>
    /// An extended spectrum type that reads mz, intensity, charge, resolution, etc. for each data point.
    /// </summary>
    private SpectrumEx spectrumEx;

    /// <summary>
    /// All lines of the file, read once by Open(). MGF has no built-in index the way mzML/mzXML
    /// do, and hand-rolling exact byte-offset seeking on top of System.IO.StreamReader is a
    /// well-known footgun (its internal buffering makes the underlying Stream.Position useless
    /// for precise re-seeking). Reading the whole file into memory once and indexing by line
    /// number sidesteps that entirely, at the cost of holding the full file in memory.
    /// </summary>
    private string[] lines = Array.Empty<string>();

    /// <summary>
    /// Resolved scan numbers in file order. Parallel to blockStartLine: scanOrder[i] is the
    /// scan number for the spectrum whose "BEGIN IONS" line is blockStartLine[i].
    /// </summary>
    private List<int> scanOrder = new List<int>();

    /// <summary>
    /// Line index (into <see cref="lines"/>) of each spectrum's "BEGIN IONS" line, parallel to
    /// <see cref="scanOrder"/>.
    /// </summary>
    private List<int> blockStartLine = new List<int>();

    /// <summary>
    /// Maps a resolved scan number back to its position in scanOrder/blockStartLine, for
    /// scan-number-based random access (GetSpectrum(scanNumber)).
    /// </summary>
    private Dictionary<int, int> scanNumberToOrderIndex = new Dictionary<int, int>();

    /// <summary>
    /// An enum bitwise operator indicating the desired spectrum levels to read. By default MS1, MS2, and MS3 are read.
    /// </summary>
    public MSFilter Filter { get; set; } = MSFilter.MS1 | MSFilter.MS2 | MSFilter.MS3;

    /// <summary>
    /// Position within scanOrder for sequential reads (GetSpectrum with no scan number given).
    /// -1 means "not started yet"; Reset() returns it here.
    /// </summary>
    private int currentOrderIndex = -1;

    /// <summary>
    /// The ScanNumber of the most recent scan that was read. A value of 0 means a scan has not yet been read.
    /// </summary>
    private int CurrentScanNumber = 0;

    /// <summary>
    /// Charge parsed from the file's global (pre-"BEGIN IONS") CHARGE= header line, used as a
    /// fallback when a spectrum block has no CHARGE of its own and PEPMASS didn't include one.
    /// Per the spec, the header value can list multiple charge states ("2+ and 3+"); Nova's
    /// PrecursorIon.Charge holds only one, so only the first listed charge is kept.
    /// </summary>
    private int globalCharge = 0;

    public int ScanCount { get; private set; } = 0;

    public int FirstScan { get; private set; } = 0;
    public int LastScan { get; private set; } = 0;
    public double MaxRetentionTime { get; private set; } = 0;

    /// <summary>
    /// Matches the common (though not Mascot-spec-mandated) msconvert-style TITLE convention
    /// "&lt;base&gt;.&lt;firstScan&gt;.&lt;lastScan&gt;." -- used as a fallback scan-number
    /// source when a spectrum has no explicit SCANS= tag, since real-world MGF files (including
    /// Test/Files/AngioNeuro4.mgf) commonly omit SCANS but carry the scan number in TITLE.
    /// </summary>
    private static readonly Regex TitleScanRegex = new Regex(@"\.(\d+)\.\d+\.\s*$", RegexOptions.Compiled);

    /// <summary>
    /// Constructor for MGFReader
    /// </summary>
    /// <param name="filter">The desired scan filter.</param>
    public MGFReader(MSFilter filter)
    {
      Filter = filter;
      spectrum = new Spectrum();
      spectrumEx = new SpectrumEx();
    }

    /// <summary>
    /// Opens an MGF file for reading. Reads the whole file into memory and builds an in-memory
    /// index of every "BEGIN IONS"/"END IONS" spectrum block, resolving each spectrum's scan
    /// number along the way (from SCANS=, falling back to the TITLE convention, falling back to
    /// sequential numbering). Does not parse peak data at this point -- that happens lazily in
    /// ParseSpectrum, only for spectra actually requested.
    /// </summary>
    /// <param name="fileName">A valid path to an MGF file.</param>
    /// <returns>true if file opened successfully and at least one spectrum was found, false otherwise.</returns>
    public bool Open(string fileName)
    {
      try
      {
        lines = File.ReadAllLines(fileName);

        scanOrder.Clear();
        blockStartLine.Clear();
        scanNumberToOrderIndex.Clear();
        globalCharge = 0;

        bool inBlock = false;
        int blockStart = -1;
        string? blockScans = null;
        string? blockTitle = null;

        for (int i = 0; i < lines.Length; i++)
        {
          string line = lines[i].Trim();
          if (line.Length == 0) continue;

          if (!inBlock)
          {
            //Comment lines are only meaningful outside a spectrum block -- the spec disallows
            //them between BEGIN IONS and END IONS entirely.
            if (line[0] == '#' || line[0] == ';' || line[0] == '!' || line[0] == '/') continue;

            if (line.StartsWith("CHARGE=", StringComparison.OrdinalIgnoreCase))
            {
              globalCharge = ParseChargeToken(line.Substring(7).Split(' ')[0]);
            }
            else if (string.Equals(line, "BEGIN IONS", StringComparison.OrdinalIgnoreCase))
            {
              inBlock = true;
              blockStart = i;
              blockScans = null;
              blockTitle = null;
            }
            continue;
          }

          //Inside a spectrum block: only track what's needed to resolve a scan number here.
          //The full per-spectrum parse (peaks, PEPMASS, CHARGE, etc.) happens lazily in
          //ParseSpectrum, only for spectra actually requested.
          if (string.Equals(line, "END IONS", StringComparison.OrdinalIgnoreCase))
          {
            int scanNumber = ResolveScanNumber(blockScans, blockTitle, scanOrder.Count + 1);
            if (!scanNumberToOrderIndex.ContainsKey(scanNumber))
            {
              scanNumberToOrderIndex[scanNumber] = scanOrder.Count;
              scanOrder.Add(scanNumber);
              blockStartLine.Add(blockStart);
            }
            //else: this spectrum's resolved scan number collided with an earlier one (e.g. two
            //blocks whose TITLE fallback produced the same number) -- keep the first, drop this
            //one from the index rather than overwriting it.
            inBlock = false;
          }
          else if (line.StartsWith("SCANS=", StringComparison.OrdinalIgnoreCase))
          {
            blockScans = line.Substring(6);
          }
          else if (line.StartsWith("TITLE=", StringComparison.OrdinalIgnoreCase))
          {
            blockTitle = line.Substring(6);
          }
        }

        if (scanOrder.Count == 0)
        {
          Console.WriteLine("Failed to open: no BEGIN IONS/END IONS spectrum blocks found.");
          return false;
        }

        ScanCount = scanOrder.Count;
        FirstScan = scanOrder[0];
        LastScan = scanOrder[scanOrder.Count - 1];

        //Read the last spectrum to get the max retention time, mirroring MzMLReader/MzXMLReader.
        ParseSpectrum(blockStartLine[blockStartLine.Count - 1], LastScan, false);
        MaxRetentionTime = spectrum.RetentionTime;
        Reset();
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Failed to open: {ex.Message}");
        return false;
      }
      return true;
    }

    public void Close()
    {
      //Nothing to release -- Open() reads the whole file up front and holds no file handle.
    }

    /// <summary>
    /// MGF files do not have chromatograms. An empty chromatogram object is returned every time.
    /// </summary>
    /// <param name="chromatIndex"></param>
    /// <returns></returns>
    public Chromatogram GetChromatogram(int chromatIndex = -1)
    {
      return new Chromatogram(0);
    }

    public Spectrum GetSpectrum(int scanNumber = -1, bool centroid = true)
    {
      int orderIndex;
      if (scanNumber < 0)
      {
        orderIndex = currentOrderIndex + 1;
      }
      else if (!scanNumberToOrderIndex.TryGetValue(scanNumber, out orderIndex))
      {
        spectrum = new Spectrum(0);
        return spectrum;
      }

      while (true)
      {
        if (orderIndex >= scanOrder.Count)
        {
          currentOrderIndex = orderIndex;
          spectrum = new Spectrum(0);
          return spectrum;
        }

        ParseSpectrum(blockStartLine[orderIndex], scanOrder[orderIndex], false);
        currentOrderIndex = orderIndex;
        CurrentScanNumber = scanOrder[orderIndex];

        bool matchScanType = spectrum.MsLevel switch
        {
          1 => Filter.HasFlag(MSFilter.MS1),
          2 => Filter.HasFlag(MSFilter.MS2),
          3 => Filter.HasFlag(MSFilter.MS3),
          _ => false
        };
        if (matchScanType) return spectrum;

        if (scanNumber >= 0)
        {
          //A specific scan number was requested but didn't pass the filter -- matches
          //MzXMLReader/MzMLReader's convention of returning empty rather than searching on.
          spectrum = new Spectrum(0);
          return spectrum;
        }
        orderIndex++;
      }
    }

    public SpectrumEx GetSpectrumEx(int scanNumber = -1, bool centroid = true)
    {
      int orderIndex;
      if (scanNumber < 0)
      {
        orderIndex = currentOrderIndex + 1;
      }
      else if (!scanNumberToOrderIndex.TryGetValue(scanNumber, out orderIndex))
      {
        spectrumEx = new SpectrumEx(0);
        return spectrumEx;
      }

      while (true)
      {
        if (orderIndex >= scanOrder.Count)
        {
          currentOrderIndex = orderIndex;
          spectrumEx = new SpectrumEx(0);
          return spectrumEx;
        }

        ParseSpectrum(blockStartLine[orderIndex], scanOrder[orderIndex], true);
        currentOrderIndex = orderIndex;
        CurrentScanNumber = scanOrder[orderIndex];

        bool matchScanType = spectrumEx.MsLevel switch
        {
          1 => Filter.HasFlag(MSFilter.MS1),
          2 => Filter.HasFlag(MSFilter.MS2),
          3 => Filter.HasFlag(MSFilter.MS3),
          _ => false
        };
        if (matchScanType) return spectrumEx;

        if (scanNumber >= 0)
        {
          spectrumEx = new SpectrumEx(0);
          return spectrumEx;
        }
        orderIndex++;
      }
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns>Spectrum object</returns>
    public IEnumerator GetEnumerator()
    {
      Reset();
      Spectrum spec = GetSpectrum();
      while (spec.ScanNumber > 0)
      {
        yield return spec;
        spec = GetSpectrum();
      }
      Reset();
    }

    /// <summary>
    /// Parses a single spectrum block starting at lines[beginIonsLine] ("BEGIN IONS") through
    /// its "END IONS" line, populating either spectrum or spectrumEx.
    /// </summary>
    /// <param name="beginIonsLine">Line index of this spectrum's "BEGIN IONS" line.</param>
    /// <param name="scanNumber">This spectrum's already-resolved scan number (see Open()).</param>
    /// <param name="extended">true to populate spectrumEx, false to populate spectrum.</param>
    private void ParseSpectrum(int beginIonsLine, int scanNumber, bool extended)
    {
      if (extended) spectrumEx = new SpectrumEx(0);
      else spectrum = new Spectrum(0);

      List<PrecursorIon> precursors = new List<PrecursorIon>();
      List<SpecDataPoint> points = new List<SpecDataPoint>();
      List<SpecDataPointEx> pointsEx = new List<SpecDataPointEx>();
      int localCharge = 0;
      bool haveLocalCharge = false;
      double retentionTimeSeconds = -1;

      for (int i = beginIonsLine + 1; i < lines.Length; i++)
      {
        string line = lines[i].Trim();
        if (line.Length == 0) continue;
        if (string.Equals(line, "END IONS", StringComparison.OrdinalIgnoreCase)) break;

        if (line.StartsWith("PEPMASS=", StringComparison.OrdinalIgnoreCase))
        {
          string[] tokens = line.Substring(8).Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
          if (tokens.Length > 0 && double.TryParse(tokens[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double mz))
          {
            //MGF reports one m/z per precursor with no distinction between an isolation target
            //and a monoisotopic value, so both are set from the same PEPMASS number.
            PrecursorIon pre = new PrecursorIon();
            pre.MonoisotopicMz = mz;
            pre.IsolationMz = mz;
            if (tokens.Length > 1 && double.TryParse(tokens[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double intensity))
            {
              pre.Intensity = intensity;
            }
            if (tokens.Length > 2)
            {
              pre.Charge = ParseChargeToken(tokens[2]);
            }
            precursors.Add(pre);
          }
        }
        else if (line.StartsWith("CHARGE=", StringComparison.OrdinalIgnoreCase))
        {
          localCharge = ParseChargeToken(line.Substring(7).Split(' ')[0]);
          haveLocalCharge = true;
        }
        else if (line.StartsWith("RTINSECONDS=", StringComparison.OrdinalIgnoreCase))
        {
          string rtValue = line.Substring(12).Split(',')[0].Split('-')[0].Trim();
          double.TryParse(rtValue, NumberStyles.Float, CultureInfo.InvariantCulture, out retentionTimeSeconds);
        }
        else if (line.StartsWith("SCANS=", StringComparison.OrdinalIgnoreCase) || line.StartsWith("TITLE=", StringComparison.OrdinalIgnoreCase))
        {
          //Already accounted for in Open()'s scan-number resolution; nothing further to do.
        }
        else if (char.IsDigit(line[0]) || line[0] == '-' || line[0] == '.')
        {
          //A fragment ion peak line: "m/z intensity [charge]".
          string[] tokens = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
          if (tokens.Length >= 2
            && double.TryParse(tokens[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double pmz)
            && double.TryParse(tokens[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double pIntensity))
          {
            if (extended)
            {
              SpecDataPointEx pt = new SpecDataPointEx(pmz, pIntensity);
              if (tokens.Length > 2) pt.Charge = ParseChargeToken(tokens[2]);
              pointsEx.Add(pt);
            }
            else
            {
              points.Add(new SpecDataPoint(pmz, pIntensity));
            }
          }
        }
        //else: an unrecognized tag (COM, TOL, INSTRUMENT, RAWFILE, etc.) with no corresponding
        //Nova.Data field -- skipped.
      }

      //PEPMASS's own charge token (if any) wins; otherwise the spectrum-local CHARGE; otherwise
      //the file's global header CHARGE.
      int effectiveCharge = haveLocalCharge ? localCharge : globalCharge;
      foreach (PrecursorIon pre in precursors)
      {
        if (pre.Charge == 0) pre.Charge = effectiveCharge;
      }

      double retentionMinutes = retentionTimeSeconds >= 0 ? retentionTimeSeconds / 60.0 : 0;

      if (extended)
      {
        spectrumEx.ScanNumber = scanNumber;
        spectrumEx.MsLevel = 2; //MGF spectra are always MS/MS -- there is no MS1 concept in the format.
        spectrumEx.Centroid = true; //MGF is inherently a peak list, never profile data.
        spectrumEx.RetentionTime = retentionMinutes;
        foreach (PrecursorIon pre in precursors) spectrumEx.Precursors.Add(pre);
        spectrumEx.Resize(pointsEx.Count);
        for (int p = 0; p < pointsEx.Count; p++) spectrumEx.DataPoints[p] = pointsEx[p];
        ProcessScanStats(true);
      }
      else
      {
        spectrum.ScanNumber = scanNumber;
        spectrum.MsLevel = 2;
        spectrum.Centroid = true;
        spectrum.RetentionTime = retentionMinutes;
        foreach (PrecursorIon pre in precursors) spectrum.Precursors.Add(pre);
        spectrum.Resize(points.Count);
        for (int p = 0; p < points.Count; p++) spectrum.DataPoints[p] = points[p];
        ProcessScanStats(false);
      }
    }

    /// <summary>
    /// Resolves a spectrum's scan number: prefers an explicit SCANS= value (taking the first
    /// number if it's a range/list, per the spec's "1280-1284,1290-1294" syntax); falls back to
    /// the common msconvert-style TITLE convention; falls back to sequential numbering if
    /// neither is present.
    /// </summary>
    private static int ResolveScanNumber(string? scans, string? title, int sequentialFallback)
    {
      if (!string.IsNullOrEmpty(scans))
      {
        string first = scans.Split(',')[0].Split('-')[0].Trim();
        if (int.TryParse(first, out int scanNumber)) return scanNumber;
      }
      if (!string.IsNullOrEmpty(title))
      {
        Match m = TitleScanRegex.Match(title);
        if (m.Success && int.TryParse(m.Groups[1].Value, out int scanNumber)) return scanNumber;
      }
      return sequentialFallback;
    }

    /// <summary>
    /// Parses a charge token in the spec's "2+"/"3-" notation (trailing sign, no leading sign)
    /// into a signed int. Returns 0 (unknown) if the token doesn't parse.
    /// </summary>
    private static int ParseChargeToken(string token)
    {
      token = token.Trim();
      if (token.Length == 0) return 0;
      bool negative = token[token.Length - 1] == '-';
      string digits = token.TrimEnd('+', '-');
      if (int.TryParse(digits, out int value)) return negative ? -value : value;
      return 0;
    }

    /// <summary>
    /// Computes TotalIonCurrent, BasePeakMz/Intensity, and the observed m/z range from the
    /// already-parsed peak data -- MGF has no header fields for any of these, unlike
    /// mzML/mzXML/RAW.
    /// </summary>
    private void ProcessScanStats(bool extended)
    {
      double tic = 0;
      double bpi = 0;
      double bpmz = 0;
      double lowMz = double.MaxValue;
      double highMz = double.MinValue;
      int count;

      if (extended)
      {
        foreach (SpecDataPointEx pt in spectrumEx.DataPoints)
        {
          tic += pt.Intensity;
          if (pt.Intensity > bpi) { bpi = pt.Intensity; bpmz = pt.Mz; }
          if (pt.Mz < lowMz) lowMz = pt.Mz;
          if (pt.Mz > highMz) highMz = pt.Mz;
        }
        count = spectrumEx.Count;
        spectrumEx.TotalIonCurrent = tic;
        spectrumEx.BasePeakIntensity = bpi;
        spectrumEx.BasePeakMz = bpmz;
        if (count > 0)
        {
          spectrumEx.LowestMz = lowMz;
          spectrumEx.HighestMz = highMz;
          spectrumEx.StartMz = lowMz;
          spectrumEx.EndMz = highMz;
        }
      }
      else
      {
        foreach (SpecDataPoint pt in spectrum.DataPoints)
        {
          tic += pt.Intensity;
          if (pt.Intensity > bpi) { bpi = pt.Intensity; bpmz = pt.Mz; }
          if (pt.Mz < lowMz) lowMz = pt.Mz;
          if (pt.Mz > highMz) highMz = pt.Mz;
        }
        count = spectrum.Count;
        spectrum.TotalIonCurrent = tic;
        spectrum.BasePeakIntensity = bpi;
        spectrum.BasePeakMz = bpmz;
        if (count > 0)
        {
          spectrum.LowestMz = lowMz;
          spectrum.HighestMz = highMz;
          spectrum.StartMz = lowMz;
          spectrum.EndMz = highMz;
        }
      }
    }

    public void Reset()
    {
      currentOrderIndex = -1;
      CurrentScanNumber = 0;
    }
  }
}

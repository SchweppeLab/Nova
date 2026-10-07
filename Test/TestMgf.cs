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

using Nova.Data;
using Nova.Io.Read;

namespace TestNova
{
  // BUG-1/BUG-2 (docs/known-issues.md): FileReader.OpenSpectrumFile had no working dispatch
  // path for .mgf, and MGFReader was a non-functional stub (Open() parsed only the global
  // header, GetSpectrum/GetSpectrumEx always returned an empty spectrum). Both fixed together
  // 2026-08-19, implemented against the Matrix Science MGF spec
  // (https://www.matrixscience.com/help/data_file_help.html). These tests exercise the real
  // implementation against Test/Files/AngioNeuro4.mgf -- the MS2-only subset of the same
  // AngioNeuro4 acquisition TestNova.cs already reads as mzML/mzXML/RAW (308 MS1 + 6 MS2 scans;
  // this file is exactly those 6 MS2 spectra). Its TITLE fields (e.g. "AngioNeuro4.16.16.")
  // carry the real underlying scan numbers via the common msconvert convention, which
  // MGFReader's scan-number resolution falls back to since this file has no SCANS= tags.
  // Expected values below (peak counts, RT, m/z range, etc.) were independently computed
  // straight from the fixture's text (line counts, awk sums), not derived from the reader
  // itself, so a bug in the reader has a real chance of being caught here rather than the test
  // just mirroring whatever the code happens to produce.
  [TestClass]
  public sealed class TestMgf
  {
    private readonly string dataFilePathMgf;
    private readonly string dataFilePathMgfMalformed;
    public TestContext testContext { get; set; }

    public TestMgf(TestContext context)
    {
      testContext = context;
      string filesDir = TestFilePaths.GetFilesDirectory();
      dataFilePathMgf = Path.Combine(filesDir, "AngioNeuro4.mgf");
      dataFilePathMgfMalformed = Path.Combine(filesDir, "AngioNeuro4Malformed.mgf");
    }

    [TestMethod]
    public void MGF_OpenAndScanCount()
    {
      testContext.WriteLine("Verifies OpenSpectrumFile succeeds on the MGF fixture (BUG-1) and reports the correct scan count/range/max retention time.");
      FileReader reader = new FileReader();
      Assert.IsTrue(reader.OpenSpectrumFile(dataFilePathMgf));
      Assert.AreEqual(6, reader.ScanCount);
      Assert.AreEqual(16, reader.FirstScan);
      Assert.AreEqual(280, reader.LastScan);
      Assert.AreEqual(37.07735426 / 60.0, reader.MaxRetentionTime, 1e-6);
    }

    [TestMethod]
    public void MGF_MsLevelTally()
    {
      testContext.WriteLine("Verifies every spectrum in the MGF fixture reads as MS2 -- MGF has no MS1 concept.");
      FileReader reader = new FileReader();
      int ms1 = 0, ms2 = 0, ms3 = 0;
      Spectrum spec = reader.ReadSpectrum(dataFilePathMgf);
      while (spec.ScanNumber > 0)
      {
        if (spec.MsLevel == 1) ms1++;
        else if (spec.MsLevel == 2) ms2++;
        else if (spec.MsLevel == 3) ms3++;
        spec = reader.ReadSpectrum();
      }
      Assert.AreEqual(0, ms1);
      Assert.AreEqual(6, ms2);
      Assert.AreEqual(0, ms3);
    }

    [TestMethod]
    public void MGF_SequentialRead_MatchesFileOrder()
    {
      testContext.WriteLine("Verifies sequential reads return the MGF fixture's 6 spectra in file order by (TITLE-derived) scan number, then an empty spectrum past the end.");
      FileReader reader = new FileReader();
      int[] expectedScanOrder = { 16, 69, 113, 179, 230, 280 };

      Spectrum spec = reader.ReadSpectrum(dataFilePathMgf);
      foreach (int expectedScan in expectedScanOrder)
      {
        Assert.AreEqual(expectedScan, spec.ScanNumber);
        spec = reader.ReadSpectrum();
      }
      Assert.AreEqual(0, spec.ScanNumber);
    }

    [TestMethod]
    public void MGF_FirstSpectrum_FieldsAndPeakData()
    {
      testContext.WriteLine("Verifies scan-level fields and peak data for the MGF fixture's first spectrum (scan 16), independently computed from the raw file text.");
      FileReader reader = new FileReader();
      Spectrum spec = reader.ReadSpectrum(dataFilePathMgf, 16);

      Assert.AreEqual(16, spec.ScanNumber);
      Assert.AreEqual(2, spec.MsLevel);
      Assert.IsTrue(spec.Centroid);
      Assert.AreEqual(2.242607424 / 60.0, spec.RetentionTime, 1e-9);
      Assert.AreEqual(3374, spec.Count);
      Assert.AreEqual(138.6049639, spec.DataPoints[0].Mz, 1e-6);
      Assert.AreEqual(0.0, spec.DataPoints[0].Intensity, 1e-6);
      Assert.AreEqual(1212.14767, spec.DataPoints[spec.Count - 1].Mz, 1e-6);
      Assert.AreEqual(138.6049639, spec.LowestMz, 1e-6);
      Assert.AreEqual(1212.14767, spec.HighestMz, 1e-6);
      Assert.AreEqual(619.354297, spec.BasePeakMz, 1e-6);
      Assert.AreEqual(93362120.0, spec.BasePeakIntensity, 1e-3);
      Assert.AreEqual(4789645202.756714, spec.TotalIonCurrent, 1.0);
    }

    [TestMethod]
    public void MGF_FirstSpectrum_PrecursorFields()
    {
      testContext.WriteLine("Verifies PEPMASS maps to a single precursor with matching isolation/monoisotopic m/z; this fixture has no intensity or charge token on PEPMASS/CHARGE, so both stay at their zero defaults.");
      FileReader reader = new FileReader();
      Spectrum spec = reader.ReadSpectrum(dataFilePathMgf, 16);

      Assert.AreEqual(1, spec.Precursors.Count);
      Assert.AreEqual(432.899993896484, spec.Precursors[0].IsolationMz, 1e-9);
      Assert.AreEqual(432.899993896484, spec.Precursors[0].MonoisotopicMz, 1e-9);
      Assert.AreEqual(0.0, spec.Precursors[0].Intensity, 1e-9);
      Assert.AreEqual(0, spec.Precursors[0].Charge);
    }

    [TestMethod]
    public void MGF_RandomAccess_ByScanNumber()
    {
      testContext.WriteLine("Verifies requesting a specific scan number (not the first) jumps directly to that spectrum instead of reading sequentially from the start.");
      FileReader reader = new FileReader();
      Spectrum spec = reader.ReadSpectrum(dataFilePathMgf, 113);
      Assert.AreEqual(113, spec.ScanNumber);
      Assert.AreEqual(8048, spec.Count);
    }

    [TestMethod]
    public void MGF_ExPath_NoPerPointChargeInFixture()
    {
      testContext.WriteLine("Verifies the Ex (SpectrumEx) read path parses the same fixture correctly; per-point Charge stays 0 since this fixture's peak lines carry no third (charge) token.");
      FileReader reader = new FileReader();
      SpectrumEx spec = reader.ReadSpectrumEx(dataFilePathMgf, 16);

      Assert.AreEqual(16, spec.ScanNumber);
      Assert.AreEqual(2, spec.MsLevel);
      Assert.AreEqual(3374, spec.Count);
      Assert.AreEqual(138.6049639, spec.DataPoints[0].Mz, 1e-6);
      Assert.AreEqual(0, spec.DataPoints[0].Charge);
    }

    [TestMethod]
    public void MGF_SpectrumFileReaderFactory_GetReader_NoLongerThrows()
    {
      testContext.WriteLine("Regression test for BUG-1/CLEAN-3: SpectrumFileReaderFactory.GetReader used to throw ArgumentException for .mgf; it now returns a working, already-opened reader.");
      ISpectrumFileReader reader = SpectrumFileReaderFactory.GetReader(dataFilePathMgf, MSFilter.MS1 | MSFilter.MS2 | MSFilter.MS3);
      Assert.AreEqual(6, reader.ScanCount);
    }

    [TestMethod]
    public void MGF_MalformedFile_OpenReturnsFalse()
    {
      testContext.WriteLine("Verifies OpenSpectrumFile returns false (not an exception, not a false 'true') for an MGF file with no BEGIN IONS/END IONS blocks at all.");
      FileReader reader = new FileReader();
      bool opened = reader.OpenSpectrumFile(dataFilePathMgfMalformed);
      Assert.IsFalse(opened);
      Assert.AreEqual(0, reader.ScanCount);
    }

    // BUG-12: a CHARGE value may list several states ("2+ and 3+", "2+,3+"), meaning the
    // precursor charge is undetermined and the spectrum should carry one precursor per state.
    // The fixtures below are written to a temporary file per test so that the expected values
    // sit beside the input text.

    private static string WriteMgf(string contents)
    {
      string path = Path.Combine(Path.GetTempPath(), "NovaTestMgf_" + Guid.NewGuid().ToString("N") + ".mgf");
      File.WriteAllText(path, contents);
      return path;
    }

    private static void AssertPrecursorCharges(Spectrum spec, double mz, double intensity, params int[] charges)
    {
      Assert.AreEqual(charges.Length, spec.Precursors.Count);
      for (int i = 0; i < charges.Length; i++)
      {
        Assert.AreEqual(charges[i], spec.Precursors[i].Charge, $"precursor {i}");
        Assert.AreEqual(mz, spec.Precursors[i].MonoisotopicMz, 1e-9, $"precursor {i}");
        Assert.AreEqual(mz, spec.Precursors[i].IsolationMz, 1e-9, $"precursor {i}");
        Assert.AreEqual(intensity, spec.Precursors[i].Intensity, 1e-9, $"precursor {i}");
      }
    }

    [DataTestMethod]
    [DataRow("2+ and 3+", new[] { 2, 3 })]
    [DataRow("2+,3+", new[] { 2, 3 })]
    [DataRow("1+, 2+ and 3+", new[] { 1, 2, 3 })]
    public void MGF_LocalChargeList_OnePrecursorPerState(string chargeValue, int[] expected)
    {
      testContext.WriteLine($"Verifies a block-local CHARGE={chargeValue} yields one precursor per listed state, in order, each with the PEPMASS m/z and intensity.");
      string path = WriteMgf(
        "BEGIN IONS\r\n" +
        "SCANS=1\r\n" +
        "PEPMASS=500.25 1234.5\r\n" +
        $"CHARGE={chargeValue}\r\n" +
        "100.0 10.0\r\n" +
        "200.0 20.0\r\n" +
        "END IONS\r\n");
      try
      {
        FileReader reader = new FileReader();
        Spectrum spec = reader.ReadSpectrum(path, 1);
        AssertPrecursorCharges(spec, 500.25, 1234.5, expected);
      }
      finally
      {
        File.Delete(path);
      }
    }

    [TestMethod]
    public void MGF_GlobalChargeList_AppliesUnlessBlockHasItsOwn()
    {
      testContext.WriteLine("Verifies a header CHARGE=2+ and 3+ gives two precursors to a block without CHARGE, while a block with CHARGE=4+ gets only that one.");
      string path = WriteMgf(
        "CHARGE=2+ and 3+\r\n" +
        "BEGIN IONS\r\n" +
        "SCANS=1\r\n" +
        "PEPMASS=500.25 1234.5\r\n" +
        "100.0 10.0\r\n" +
        "END IONS\r\n" +
        "BEGIN IONS\r\n" +
        "SCANS=2\r\n" +
        "PEPMASS=600.5 42.0\r\n" +
        "CHARGE=4+\r\n" +
        "100.0 10.0\r\n" +
        "END IONS\r\n");
      try
      {
        FileReader reader = new FileReader();
        AssertPrecursorCharges(reader.ReadSpectrum(path, 1), 500.25, 1234.5, 2, 3);
        AssertPrecursorCharges(reader.ReadSpectrum(path, 2), 600.5, 42.0, 4);
      }
      finally
      {
        File.Delete(path);
      }
    }

    [TestMethod]
    public void MGF_PepmassCharge_WinsOverChargeList()
    {
      testContext.WriteLine("Verifies a charge given on the PEPMASS line is kept as the single precursor even when CHARGE lists several states.");
      string path = WriteMgf(
        "BEGIN IONS\r\n" +
        "SCANS=1\r\n" +
        "PEPMASS=500.25 1234.5 2+\r\n" +
        "CHARGE=2+ and 3+\r\n" +
        "100.0 10.0\r\n" +
        "END IONS\r\n");
      try
      {
        FileReader reader = new FileReader();
        AssertPrecursorCharges(reader.ReadSpectrum(path, 1), 500.25, 1234.5, 2);
      }
      finally
      {
        File.Delete(path);
      }
    }
  }
}

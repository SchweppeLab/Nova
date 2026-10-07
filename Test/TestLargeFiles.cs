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
  // Byte offsets into an mzML/mzXML were parsed and stored as int, so any file past
  // 2 GiB (int.MaxValue = 2,147,483,647) threw OverflowException while reading its own index.
  // Because the open failure was then discarded, every scan came back with zero peaks
  // and the caller could not tell that from "that scan isn't in this file".
  //
  // These tests generate a sparse mzML at test time rather than reading a checked-in file --
  // see LargeMzMLFixture for why that costs essentially nothing and for the safety gate that
  // keeps it from ever materializing gigabytes for real. Nothing is added to Test/Files/.
  //
  // The 4.5 GiB case is not redundant with the 2.5 GiB one: an offset stored as *unsigned*
  // 32-bit would pass between 2 and 4 GiB and still fail beyond it, so only the larger fixture
  // distinguishes a real 64-bit fix from a half-fix.
  [TestClass]
  public sealed class TestLargeFiles
  {
    private const long TwoGiB = 2147483648L;   // 2^31, where an int offset overflows
    private const long FourGiB = 4294967296L;  // 2^32, where a uint offset would overflow

    /// <summary>
    /// Set this environment variable (to anything other than empty, "0" or "false") to turn an
    /// unavailable large-file fixture into a test *failure* instead of an Inconclusive skip.
    /// <para>
    /// The skip exists so these tests degrade gracefully where sparse files aren't available --
    /// a non-NTFS temp volume, a non-Windows run. But a skip still reports green, which means CI
    /// could quietly stop covering the overflow after, say, a runner image change and nobody would
    /// notice. That is uncomfortably close to the defect this whole area exists to prevent. All
    /// three workflows set this, so in CI an unavailable fixture is loud; locally it stays a skip.
    /// </para>
    /// </summary>
    private const string RequireEnvVar = "NOVA_REQUIRE_LARGE_FILE_TESTS";

    private static bool LargeFileTestsRequired
    {
      get
      {
        string? value = Environment.GetEnvironmentVariable(RequireEnvVar);
        if (string.IsNullOrWhiteSpace(value)) return false;
        value = value.Trim();
        //Treat an explicit "0"/"false" as off, so setting it to 0 does the obvious thing
        //rather than the opposite.
        return !value.Equals("0", StringComparison.Ordinal)
            && !value.Equals("false", StringComparison.OrdinalIgnoreCase);
      }
    }

    public TestContext testContext { get; set; }

    public TestLargeFiles(TestContext context)
    {
      testContext = context;
    }

    /// <summary>
    /// Builds the fixture, or ends the test -- as a failure when <see cref="RequireEnvVar"/> is
    /// set, otherwise as an Inconclusive skip. Never returns null in practice; both exit paths
    /// throw.
    /// </summary>
    private LargeMzMLFixture? CreateOrSkip(long logicalSize, long[] offsets)
    {
      if (LargeMzMLFixture.TryCreate(logicalSize, offsets, out var fixture, out string skip))
      {
        ReportFixture(fixture!);
        return fixture;
      }

      if (LargeFileTestsRequired)
      {
        Assert.Fail(
          $"Large-file fixture unavailable: {skip} {RequireEnvVar} is set, so this is a failure " +
          "rather than a skip -- the >2 GiB regression coverage would otherwise have been " +
          "silently lost while the build still reported green.");
      }

      testContext.WriteLine($"SKIPPED: {skip} (set {RequireEnvVar} to make this a failure instead.)");
      Assert.Inconclusive(skip);
      return null;
    }

    /// <summary>
    /// Spectrum positions for the ~2.5 GiB fixture: one well below the 2^31 boundary, one just
    /// below it, and one past it. The index and trailer land past it too, by construction.
    /// </summary>
    private static long[] OffsetsAcross2GiB => new[] { 4096L, TwoGiB - 65536, TwoGiB + 4096 };

    /// <summary>
    /// Spectrum positions for the ~4.5 GiB fixture, adding one past the 2^32 boundary.
    /// </summary>
    private static long[] OffsetsAcross4GiB => new[] { 4096L, TwoGiB + 4096, FourGiB - 65536, FourGiB + 4096 };

    [TestMethod]
    public void LargeMzML_Over2GiB_EveryScanReadsCorrectly()
    {
      testContext.WriteLine("Builds a ~2.5 GiB indexed mzML with spectra below and above the 2^31 byte boundary, and verifies every scan reads back with its exact peaks. Before the fix, opening this file threw OverflowException and every scan returned 0 peaks.");
      RunFixture(2684354560L, OffsetsAcross2GiB);
    }

    [TestMethod]
    public void LargeMzML_Over4GiB_EveryScanReadsCorrectly()
    {
      testContext.WriteLine("Builds a ~4.5 GiB indexed mzML with a spectrum past the 4 GiB boundary, which an unsigned-32-bit offset would still fail on. Verifies every scan reads back with its exact peaks.");
      RunFixture(4831838208L, OffsetsAcross4GiB);
    }

    [TestMethod]
    public void LargeMzML_Over2GiB_FactoryReturnsWorkingReader()
    {
      testContext.WriteLine("The path from the field report: SpectrumFileReaderFactory.GetReader on a >2 GiB mzML must return a reader that yields real spectra, not one that silently returns 0 peaks for every scan.");

      using (var fixture = CreateOrSkip(2684354560L, OffsetsAcross2GiB))
      {
        ISpectrumFileReader reader = SpectrumFileReaderFactory.GetReader(
          fixture!.Path, MSFilter.MS1 | MSFilter.MS2 | MSFilter.MS3);

        foreach (int scan in fixture.ScanNumbers)
        {
          Spectrum spec = reader.GetSpectrum(scan, true);
          Assert.AreEqual(scan, spec.ScanNumber, $"wrong scan number returned for scan {scan}");
          Assert.AreEqual(3, spec.Count, $"scan {scan} came back with {spec.Count} peaks instead of 3");
        }
        reader.Close();
      }
    }

    [TestMethod]
    public void LargeMzML_Over2GiB_IndexListOffsetItselfExceedsInt32()
    {
      testContext.WriteLine("Pins down the specific value that used to overflow: the fixture's own indexListOffset must be past int.MaxValue, otherwise these tests would pass even against the unfixed code.");

      using (var fixture = CreateOrSkip(2684354560L, OffsetsAcross2GiB))
      {
        Assert.IsTrue(fixture!.IndexListOffset > int.MaxValue,
          $"fixture indexListOffset {fixture.IndexListOffset} does not exceed int.MaxValue, so it would not reproduce the overflow");
        Assert.IsTrue(fixture.Offsets.Any(o => o > int.MaxValue),
          "fixture has no spectrum past int.MaxValue, so it would not reproduce the overflow");
      }
    }

    /// <summary>
    /// Opens the fixture through the public FileReader facade and asserts every scan comes back
    /// with its exact scan number, MS level, retention time, peak count, m/z and intensities.
    /// </summary>
    private void RunFixture(long logicalSize, long[] offsets)
    {
      using (var fixture = CreateOrSkip(logicalSize, offsets))
      {
        FileReader reader = new FileReader();
        Assert.IsTrue(reader.OpenSpectrumFile(fixture!.Path),
          "OpenSpectrumFile failed on the large fixture");
        Assert.AreEqual(fixture.ScanNumbers.Length, reader.ScanCount, "wrong scan count");
        Assert.AreEqual(fixture.ScanNumbers.First(), reader.FirstScan, "wrong first scan");
        Assert.AreEqual(fixture.ScanNumbers.Last(), reader.LastScan, "wrong last scan");

        for (int i = 0; i < fixture.ScanNumbers.Length; i++)
        {
          int scan = fixture.ScanNumbers[i];
          long offset = fixture.Offsets[i];
          Spectrum spec = reader.ReadSpectrum(fixture.Path, scan);

          testContext.WriteLine(
            $"  scan {scan} at byte {offset:N0}{(offset > int.MaxValue ? " (past int.MaxValue)" : "")}: " +
            $"{spec.Count} peaks, ms{spec.MsLevel}, rt={spec.RetentionTime:N3}");

          Assert.AreEqual(scan, spec.ScanNumber, $"wrong scan number for scan {scan}");
          Assert.AreEqual(2, spec.MsLevel, $"wrong MS level for scan {scan}");
          Assert.AreEqual(LargeMzMLFixture.ExpectedRetentionTime(scan), spec.RetentionTime, 1e-6,
            $"wrong retention time for scan {scan}");
          Assert.AreEqual(3, spec.Count, $"scan {scan} came back with {spec.Count} peaks instead of 3");

          double[] expectedMz = LargeMzMLFixture.ExpectedMz(scan);
          double[] expectedIn = LargeMzMLFixture.ExpectedIntensity(scan);
          for (int p = 0; p < 3; p++)
          {
            Assert.AreEqual(expectedMz[p], spec.DataPoints[p].Mz, 1e-9,
              $"wrong m/z at peak {p} of scan {scan}");
            Assert.AreEqual(expectedIn[p], spec.DataPoints[p].Intensity, 1e-9,
              $"wrong intensity at peak {p} of scan {scan}");
          }
        }
      }
    }

    private void ReportFixture(LargeMzMLFixture fixture)
    {
      testContext.WriteLine(
        $"  fixture: {new FileInfo(fixture.Path).Length:N0} bytes logical, " +
        $"{fixture.AllocatedBytes:N0} bytes allocated on disk, " +
        $"indexListOffset {fixture.IndexListOffset:N0}");
    }
  }
}

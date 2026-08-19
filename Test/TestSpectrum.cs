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

namespace TestNova
{
  // TEST-1: Nova core has no unit test coverage at all. These tests exercise TSpectrum<T>.GetMz
  // (binary search + ppm-tolerance boundary logic, see CLEAN-1) and the Serialize/Deserialize
  // round trip, independent of any file I/O.
  [TestClass]
  public sealed class TestSpectrum
  {
    private static Spectrum BuildSpectrum(params double[] mzs)
    {
      Spectrum spec = new Spectrum(mzs.Length);
      for (int a = 0; a < mzs.Length; a++)
      {
        spec.DataPoints[a] = new SpecDataPoint(mzs[a], 100.0 * (a + 1));
      }
      return spec;
    }

    [TestMethod]
    public void GetMz_EmptySpectrum_ReturnsMinusOne()
    {
      Spectrum spec = new Spectrum(0);
      Assert.AreEqual(-1, spec.GetMz(500.0, 10));
    }

    [TestMethod]
    public void GetMz_ExactMatch_ReturnsIndex()
    {
      Spectrum spec = BuildSpectrum(100.0, 200.0, 300.0, 400.0, 500.0);
      Assert.AreEqual(2, spec.GetMz(300.0));
    }

    [TestMethod]
    public void GetMz_WithinPpmTolerance_ReturnsNearestIndex()
    {
      Spectrum spec = BuildSpectrum(100.0, 200.0, 300.0, 400.0, 500.0);
      // 300.002 is not an exact match, but is within a 10ppm window of the 300.0 data point.
      Assert.AreEqual(2, spec.GetMz(300.002, 10));
    }

    [TestMethod]
    public void GetMz_OutsidePpmTolerance_ReturnsMinusOne()
    {
      Spectrum spec = BuildSpectrum(100.0, 200.0, 300.0, 400.0, 500.0);
      Assert.AreEqual(-1, spec.GetMz(300.5, 10));
    }

    [TestMethod]
    public void GetMz_BelowFirstPoint_OutsideTolerance_ReturnsMinusOne()
    {
      Spectrum spec = BuildSpectrum(100.0, 200.0, 300.0);
      Assert.AreEqual(-1, spec.GetMz(50.0, 10));
    }

    [TestMethod]
    public void GetMz_BelowFirstPoint_WithinTolerance_ReturnsFirstIndex()
    {
      Spectrum spec = BuildSpectrum(100.0, 200.0, 300.0);
      // 99 is below the first point (100.0), but a wide-enough ppm window still covers it.
      Assert.AreEqual(0, spec.GetMz(99.0, 15000));
    }

    [TestMethod]
    public void GetMz_AboveLastPoint_WithinTolerance_ReturnsLastIndex()
    {
      Spectrum spec = BuildSpectrum(100.0, 200.0, 300.0, 400.0, 500.0);
      // 501 is past the end of the array, but within a wide-enough ppm window of the last point.
      Assert.AreEqual(4, spec.GetMz(501.0, 2000));
    }

    [TestMethod]
    public void GetMz_AboveLastPoint_OutsideTolerance_ReturnsMinusOne()
    {
      Spectrum spec = BuildSpectrum(100.0, 200.0, 300.0, 400.0, 500.0);
      Assert.AreEqual(-1, spec.GetMz(600.0, 1));
    }

    [TestMethod]
    public void GetMz_SinglePointSpectrum_ExactMatch()
    {
      Spectrum spec = BuildSpectrum(250.0);
      Assert.AreEqual(0, spec.GetMz(250.0));
    }

    [TestMethod]
    public void GetMz_ZeroPpm_OnlyExactMatchesSucceed()
    {
      Spectrum spec = BuildSpectrum(100.0, 200.0, 300.0);
      Assert.AreEqual(-1, spec.GetMz(250.0, 0));
      Assert.AreEqual(1, spec.GetMz(200.0, 0));
    }

    [TestMethod]
    public void SerializeDeserialize_RoundTrip_Spectrum()
    {
      Spectrum original = new Spectrum(2);
      original.ScanNumber = 42;
      original.MsLevel = 2;
      original.Centroid = false;
      original.RetentionTime = 12.34;
      original.StartMz = 100.0;
      original.EndMz = 1000.0;
      original.TotalIonCurrent = 5.5e6;
      original.BasePeakIntensity = 1.2e5;
      original.FaimsState = true;
      original.FaimsCV = -45.0;
      original.Analyzer = "FTMS";
      original.IonInjectionTime = 25.5;
      original.ScanType = "Full";
      original.PrecursorMasterScanNumber = 41;
      original.MasterIndex = 7;
      original.ScanFilter = "FTMS + p NSI Full ms2 500.25@hcd28.00";
      original.Precursors.Add(new PrecursorIon { IsolationMz = 500.25, IsolationWidth = 1.6, MonoisotopicMz = 500.20, Charge = 2 });
      original.DataPoints[0] = new SpecDataPoint(400.1, 1000.0);
      original.DataPoints[1] = new SpecDataPoint(500.2, 2000.0);

      byte[] bytes = original.Serialize();

      Spectrum roundTripped = new Spectrum();
      roundTripped.Deserialize(bytes);

      Assert.AreEqual(original.ScanNumber, roundTripped.ScanNumber);
      Assert.AreEqual(original.MsLevel, roundTripped.MsLevel);
      Assert.AreEqual(original.Centroid, roundTripped.Centroid);
      Assert.AreEqual(original.RetentionTime, roundTripped.RetentionTime);
      Assert.AreEqual(original.StartMz, roundTripped.StartMz);
      Assert.AreEqual(original.EndMz, roundTripped.EndMz);
      Assert.AreEqual(original.TotalIonCurrent, roundTripped.TotalIonCurrent);
      Assert.AreEqual(original.BasePeakIntensity, roundTripped.BasePeakIntensity);
      Assert.AreEqual(original.FaimsState, roundTripped.FaimsState);
      Assert.AreEqual(original.FaimsCV, roundTripped.FaimsCV);
      Assert.AreEqual(original.Analyzer, roundTripped.Analyzer);
      Assert.AreEqual(original.IonInjectionTime, roundTripped.IonInjectionTime);
      Assert.AreEqual(original.ScanType, roundTripped.ScanType);
      Assert.AreEqual(original.PrecursorMasterScanNumber, roundTripped.PrecursorMasterScanNumber);
      Assert.AreEqual(original.MasterIndex, roundTripped.MasterIndex);
      Assert.AreEqual(original.ScanFilter, roundTripped.ScanFilter);

      Assert.AreEqual(original.Precursors.Count, roundTripped.Precursors.Count);
      Assert.AreEqual(original.Precursors[0].IsolationMz, roundTripped.Precursors[0].IsolationMz);
      Assert.AreEqual(original.Precursors[0].IsolationWidth, roundTripped.Precursors[0].IsolationWidth);
      Assert.AreEqual(original.Precursors[0].MonoisotopicMz, roundTripped.Precursors[0].MonoisotopicMz);
      Assert.AreEqual(original.Precursors[0].Charge, roundTripped.Precursors[0].Charge);

      Assert.AreEqual(original.Count, roundTripped.Count);
      Assert.AreEqual(original.DataPoints[0].Mz, roundTripped.DataPoints[0].Mz);
      Assert.AreEqual(original.DataPoints[0].Intensity, roundTripped.DataPoints[0].Intensity);
      Assert.AreEqual(original.DataPoints[1].Mz, roundTripped.DataPoints[1].Mz);
      Assert.AreEqual(original.DataPoints[1].Intensity, roundTripped.DataPoints[1].Intensity);
    }

    [TestMethod]
    public void SerializeDeserialize_RoundTrip_SpectrumEx()
    {
      SpectrumEx original = new SpectrumEx(1);
      original.ScanNumber = 99;
      original.MsLevel = 1;
      original.Analyzer = "ITMS";
      original.DataPoints[0] = new SpecDataPointEx(mz: 321.5, intensity: 4321.0, noise: 12.5, baseline: 3.1, charge: 3, resolution: 60000.0);

      byte[] bytes = original.Serialize();

      SpectrumEx roundTripped = new SpectrumEx();
      roundTripped.Deserialize(bytes);

      Assert.AreEqual(original.ScanNumber, roundTripped.ScanNumber);
      Assert.AreEqual(original.MsLevel, roundTripped.MsLevel);
      Assert.AreEqual(original.Analyzer, roundTripped.Analyzer);
      Assert.AreEqual(original.Count, roundTripped.Count);
      Assert.AreEqual(original.DataPoints[0].Mz, roundTripped.DataPoints[0].Mz);
      Assert.AreEqual(original.DataPoints[0].Intensity, roundTripped.DataPoints[0].Intensity);
      Assert.AreEqual(original.DataPoints[0].Noise, roundTripped.DataPoints[0].Noise);
      Assert.AreEqual(original.DataPoints[0].Baseline, roundTripped.DataPoints[0].Baseline);
      Assert.AreEqual(original.DataPoints[0].Charge, roundTripped.DataPoints[0].Charge);
      Assert.AreEqual(original.DataPoints[0].Resolution, roundTripped.DataPoints[0].Resolution);
    }
  }
}

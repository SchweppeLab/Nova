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
using Nova.Io.Write;
using Nova.Io.Read;
using System.IO;
using ThermoFisher.CommonCore.Data;

namespace TestNova
{
  [TestClass]
  public sealed class TestNova
  {
    private readonly FileReader? Reader;
    private SpectrumEx? Spec;
    private readonly string dataFilePathMzML;
    private readonly string dataFilePathMzXML;
    private readonly string dataFilePathRaw;
    public TestContext testContext { get; set; }

    public TestNova(TestContext context)
    {
      Reader = new FileReader();
      testContext = context;
      string dir = string.Empty;
      string curDir = Environment.CurrentDirectory;
      //A lot of ridiculousness to avoid CS8602...
      if (!curDir.IsNullOrEmpty())
      {
        DirectoryInfo? dirInfo = Directory.GetParent(curDir);
        if(dirInfo != null)
        {
          if(dirInfo.Parent != null && dirInfo.Parent.Parent != null)
          {
            dir = dirInfo.Parent.Parent.FullName;
          }
        }
        
      }
      dataFilePathMzML = Path.Combine(dir, "Files", @"AngioNeuro4.mzML");
      dataFilePathMzXML = Path.Combine(dir, "Files", @"AngioNeuro4.mzXML");
      dataFilePathRaw = Path.Combine(dir, "Files", @"AngioNeuro4.raw");
    }

    [TestMethod]
    public void TestOpenFile()
    {
      if (Reader == null) Assert.Fail();

      bool bMzML = Reader.OpenSpectrumFile(dataFilePathMzML);
      Assert.AreEqual(true,bMzML);
      int scMzML = Reader.ScanCount;
      Assert.AreEqual(314, scMzML);

      bool bMzXML = Reader.OpenSpectrumFile(dataFilePathMzXML);
      Assert.AreEqual(true, bMzXML);
      int scMzXML = Reader.ScanCount;
      Assert.AreEqual(314, scMzXML);

      bool bRaw = Reader.OpenSpectrumFile(dataFilePathRaw);
      Assert.AreEqual(true, bRaw);
      int scRaw = Reader.ScanCount;
      Assert.AreEqual(314, scRaw);
    }

    [TestMethod]
    public void TestReadSpectrumEx()
    {
      
      if(Reader == null) Assert.Fail();
      
      Spec = Reader.ReadSpectrumEx(dataFilePathMzML);
      if( Spec == null ) Assert.Fail();
      Assert.AreEqual(406,Spec.Count);
      Assert.AreEqual(1,Spec.MsLevel);

      Spec = Reader.ReadSpectrumEx(dataFilePathMzXML);
      if (Spec == null) Assert.Fail();
      Assert.AreEqual(406, Spec.Count);
      Assert.AreEqual(1, Spec.MsLevel);

      Spec = Reader.ReadSpectrumEx(dataFilePathRaw);
      if (Spec == null) Assert.Fail();
      Assert.AreEqual(398, Spec.Count); //Thermo raw file reader produces a different centroid count that the previous conversions from ProteoWizard
      Assert.AreEqual(1, Spec.MsLevel);
    }

    [TestMethod]
    public void TestSpecTally()
    {
      int ms1 = 0;
      int ms2 = 0;
      int ms3 = 0;
      if (Reader == null) Assert.Fail();
      
      Spectrum spec = Reader.ReadSpectrum(dataFilePathMzML);
      while (spec.ScanNumber > 0)
      {
        if (spec.MsLevel == 1) ms1++;
        else if (spec.MsLevel == 2) ms2++;
        else if (spec.MsLevel == 3) ms3++;
        spec = Reader.ReadSpectrum();
      }

      Assert.AreEqual(308,ms1);
      Assert.AreEqual(6,ms2);
      Assert.AreEqual(0,ms3);

      ms1 = 0;
      ms2 = 0;
      ms3 = 0;
      spec = Reader.ReadSpectrum(dataFilePathMzXML);
      while (spec.ScanNumber > 0)
      {
        if (spec.MsLevel == 1) ms1++;
        else if (spec.MsLevel == 2) ms2++;
        else if (spec.MsLevel == 3) ms3++;
        spec = Reader.ReadSpectrum();
      }

      Assert.AreEqual(308,ms1);
      Assert.AreEqual(6,ms2);
      Assert.AreEqual(0,ms3);
      
      ms1 = 0;
      ms2 = 0;
      ms3 = 0;
      spec = Reader.ReadSpectrum(dataFilePathRaw);
      while (spec.ScanNumber > 0)
      {
        if (spec.MsLevel == 1) ms1++;
        else if (spec.MsLevel == 2) ms2++;
        else if (spec.MsLevel == 3) ms3++;
        spec = Reader.ReadSpectrum();
      }

      Assert.AreEqual(308, ms1);
      Assert.AreEqual(6, ms2);
      Assert.AreEqual(0, ms3);
    }

  }
}

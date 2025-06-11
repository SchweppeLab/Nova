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

#define GLOBALMEM

using System;
using Nova.Data;
using Nova.Io.Write;
using Nova.Io.Read;
using System.Diagnostics;
using System.Collections.Specialized;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

//using SpectrumNew2 = Nova.Data.TSpectrum<Nova.Data.DataPoint>;
//using SpectrumNewEx2 = Nova.Data.TSpectrum<Nova.Data.DataPointEx>;

class NovaApp
{

  public const int ScanCount= 100000;
  public const int ScanSize = 5000;
  public const bool Interrupt = false;
  public const bool Echo = false;

  static void Main(string[] args)
  {
    Console.WriteLine("First test opens the file with the FileReader class and outputs the scan header of the first MS2 scan.");
    Console.WriteLine("The test then proceeds to count all MS2 and MS3 scans in the file.");

    //Test code accepts a Thermo raw file as a parameter, then reads all MS2 & MS3 scans from the file.
    FileReader Reader = new FileReader(MSFilter.MS2 | MSFilter.MS3);
    SpectrumEx Spec = new SpectrumEx();

    //Read a chromatogram
    Console.WriteLine("Chromatogram (first 10 points):");
    Chromatogram ch = Reader.ReadChromatogram(args[0]);
    Console.WriteLine(ch.ID);
    for (int i = 0; i < 10; i++)
    {
      Console.WriteLine(ch.DataPoints[i].RT.ToString() + "  " + ch.DataPoints[i].Intensity.ToString());
    }

    //Reading the first spectrum from a file is easy, just give it the file name.
    //If filters are used, the reader automatically advances to the first spectrum
    //that satisfies the filter. This is in a try block because exceptions regarding
    //the existence of the file can be thrown. But I'm not convinced of the utility.
    try
    {
      Spec = Reader.ReadSpectrumEx(args[0]);
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex.Message);
    }

    //A ScanNumber of 0 indicates failure to read a scan. This could be for various
    //reasons: end of file, no scan data, no scans fit filter parameters, failure to
    //open the file, etc.
    int count = 0;
    while (Spec.ScanNumber > 0)
    {
      //TODO: whatever you want to do with a spectrum...
      //Display the first spectrum header to screen
      if(count == 0){
        Console.WriteLine("ScanNumber:         " + Spec.ScanNumber.ToString());
        Console.WriteLine("ScanFilter:         " + Spec.ScanFilter);
        Console.WriteLine("Retention Time:     " + Spec.RetentionTime.ToString());
        Console.WriteLine("MS Level:           " + Spec.MsLevel.ToString());
        Console.WriteLine("Centroid:           " + (Spec.Centroid ? "yes" : "no"));
        Console.WriteLine("Base Peak Mz:       " + Spec.BasePeakMz.ToString());
        Console.WriteLine("Base Peak Int:      " + Spec.BasePeakIntensity.ToString());
        Console.WriteLine("Master ScanNumber:  " + Spec.PrecursorMasterScanNumber.ToString());
        if (Spec.MsLevel > 1)
        {
          for (int i = 0; i < Spec.Precursors.Count; i++) {
            Console.WriteLine("Precursor {0}:", i);
            Console.WriteLine("  Isolation Mz:    " + Spec.Precursors[i].IsolationMz.ToString());
            Console.WriteLine("  Isolation Width: " + Spec.Precursors[i].IsolationWidth.ToString());
            Console.WriteLine("  Monoisotopic Mz: " + Spec.Precursors[i].MonoisotopicMz.ToString());
            Console.WriteLine("  Charge:          " + Spec.Precursors[i].Charge.ToString());
          }
        }
        Console.WriteLine("Data points:    " + Spec.Count.ToString());
        foreach(var dp in Spec.DataPoints)
        {
          Console.WriteLine(dp.Mz.ToString() + " " + dp.Intensity.ToString());
        }
        
      }
      count++;

      //Reading the next spectrum is just another call to ReadSpectrum() without any parameters.
      Spec = Reader.ReadSpectrumEx();
    }

    //Give some output to indicate success.
    Console.WriteLine(count.ToString() + " MS2 and MS3 scans in " + args[0]);

    Pause();

    //Start the 2nd test
    Console.WriteLine(Environment.NewLine+"Second test opens the file with the ISpectrumFileReader interface directly, with MS2 filter applied.");
    Console.WriteLine("This interface contains an enumerator for the lazy folks out there. Ten MS2 Scan Filter lines are read.");
    Console.WriteLine("Then an attempt is made to random-access two scan numbers, an MS2 scan and an MS1 scan. Because MS1 is");
    Console.WriteLine("not in our filter, the result of attempting to read the MS1 scan is an empty spectrum.");

    //Set up the new SpectrumFileReader interface. Eventually a factory will be developed once we have more than one file reader.
    MSFilter filter = new MSFilter();
    filter = MSFilter.MS2;
    ISpectrumFileReader Reader2 = SpectrumFileReaderFactory.GetReader(args[0],filter);
    //Reader2.Open(args[0]);

    //Read the first 10 MS2 filter lines.
    count = 0;
    foreach(Spectrum s in Reader2)
    {
      Console.WriteLine(s.ScanNumber.ToString() + " " + s.ScanFilter);
      count++;
      if (count>10) break;
    }

    //Try reading two spectra, and MS2 and an MS1.
    Spec = Reader2.GetSpectrumEx(29);  //note that random-access for a specific spectrum must fall within our filter. Otherwise an empty spectrum is returned
    Console.WriteLine(Environment.NewLine + Spec.ScanNumber.ToString() + " " + Spec.ScanFilter + " (Peaks = " + Spec.Count+", first 5 shown)");
    for (int i = 0; i < Spec.Count && i < 5; i++)
    {
      Console.WriteLine("\tm/z: " + Spec.DataPoints[i].Mz.ToString() + "  abun: " + Spec.DataPoints[i].Intensity.ToString());
    }
    Spec = Reader2.GetSpectrumEx(30);  //see...told you so. 892 is MS1, but filter is for MS2
    Console.WriteLine(Environment.NewLine + Spec.ScanNumber.ToString() + " " + Spec.ScanFilter + " (Peaks = " + Spec.Count + ")");

    Pause();

    Console.WriteLine(Environment.NewLine + "Third test opens the file with the FileReader then exports it to an mzML with the MzMLWriter.");
    Console.WriteLine("Optionally the file is named to any additional command line parameter given. Otherwise 'testWriter' is used for the filename.");

    string outFile = "testWriter";
    if(args.Length>1) outFile = args[1];
    MzMLWriter mzMLWriter = new MzMLWriter();
    mzMLWriter.AddInstrumentConfiguration("IC1",null);
    mzMLWriter.AddRun(outFile,"IC1");

    Reader.Reset(); //Reset the reader to the beginning of the file.
    Reader.SetFilter(MSFilter.MS1 | MSFilter.MS2 | MSFilter.MS3);
    Spectrum Spec2 = Reader.ReadSpectrum(args[0]);
    while (Spec2.ScanNumber > 0)
    {
      mzMLWriter.AddSpectrum(Spec2);
      Spec2 = Reader.ReadSpectrum();
    }
    mzMLWriter.Write(outFile+".mzML");

  }

  public static void Pause()
  {
    Console.WriteLine("Press SPACE to continue");
    do
    {

    } while (Console.ReadKey(true).Key != ConsoleKey.Spacebar);
  }
}
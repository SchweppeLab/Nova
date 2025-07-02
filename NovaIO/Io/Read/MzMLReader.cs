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

using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System.Collections;
using System.Xml;

using Nova.Data;
using System.Runtime.InteropServices;
using System;

namespace Nova.Io.Read
{

  internal enum BinaryArrayType
  {
    Unknown,
    Mz,
    Intensity,
    Time,
    NonStandard
  }

  internal class MzMLReader : ISpectrumFileReader
  {

    private Chromatogram chromatogram;

    /// <summary>
    /// A basic spectrum type reading only mz and intensity values for each data point.
    /// </summary>
    private Spectrum spectrum;

    /// <summary>
    /// An extended spectrum type that reads mz, intensity, charge, resolution, etc. for each data point.
    /// </summary>
    private SpectrumEx spectrumEx;

    /// <summary>
    /// The XmlReader interface to the mzML file.
    /// </summary>
    private XmlReader? XmlFile;

    /// <summary>
    /// The mzML FileStream, which is essential for random access.
    /// </summary>
    private FileStream? XmlFS;

    /// <summary>
    /// List of offsets for each spectrum in the mzML file. The position in the index equals the scan number, and a value of zero
    /// indicates the scan number is not in the mzML file.
    /// </summary>
    private List<int> scanIndex = new List<int>();

    private List<int> chrIndex = new List<int>();

    /// <summary>
    /// An enum bitwise operator indicating the desired spectrum levels to read. By default MS1, MS2, and MS3 are read.
    /// </summary>
    public MSFilter Filter { get; set; } = MSFilter.MS1 | MSFilter.MS2 | MSFilter.MS3;

    private int firstScanNumber { get; set; } = 0;

    /// <summary>
    /// The ScanNumber of the last scan in the file.
    /// </summary>
    private int lastScanNumber { get; set; } = 0;
    /// <summary>
    /// The ScanNumber of the most recent scan that was read. A value of 0 means a scan has not yet been read.
    /// </summary>
    private int CurrentScanNumber = 0;

    private int CurrentChromatIndex = -1;
    private int lastChromatIndex { get; set; } = 0;

    /// <summary>
    /// Number of bits (64 if true, 32 otherwise) per value in a binary data array
    /// </summary>
    private bool bit64 = false;

    /// <summary>
    /// True if binary data array is zlib compressed.
    /// </summary>
    private bool zlib = false;

    private BinaryArrayType arrayType = BinaryArrayType.Unknown;

    /// <summary>
    /// For storing precursor ion information while parsing the mzML file.
    /// </summary>
    private PrecursorIon precursorIon;

    private bool hasMonoMz = false;

    public int ScanCount { get; private set; } = 0;

    public int FirstScan { get; private set; } = 0;
    public int LastScan { get; private set; } = 0;
    public double MaxRetentionTime { get; private set; } = 0;

    public int ChromatCount { get; private set; } = 0;

    /// <summary>
    /// Constructor for MzMLReader
    /// </summary>
    /// <param name="filter">The desired scan filter.</param>
    public MzMLReader(MSFilter filter)
    {
      Filter = filter;
      spectrum = new Spectrum();
      spectrumEx = new SpectrumEx();
      precursorIon = new PrecursorIon();
      chromatogram = new Chromatogram();
    }

    /// <summary>
    /// Opens a Thermo RAW file for reading, setting the current position to the beginning of the file and identifying the total number of spectra.
    /// </summary>
    /// <param name="fileName">A valid path to an mzML file.</param>
    /// <returns>true if file opened successfully, false otherwise.</returns>
    public bool Open(string fileName)
    {
      try
      {
        //Get the offset of the index.
        //TODO: Check to make sure the mzML is indeed indexed.
        int offset = 0;
        XmlFS = new FileStream(fileName, FileMode.Open, FileAccess.Read);
        byte[] bytes = new byte[200];
        XmlFS.Seek(-200, SeekOrigin.End);
        XmlFS.Read(bytes, offset, 200);
        string block = System.Text.Encoding.Default.GetString(bytes);
        int indexA = block.IndexOf("<indexListOffset>");
        int indexB = block.IndexOf("</indexListOffset>");
        if (indexA < 0 || indexB < 0)
        {
          throw new Exception("No index found. Please index your mzXML file.");
        }
        offset = int.Parse(block.Substring(indexA + 17, indexB - indexA - 17));

        //read the whole damn index
        scanIndex.Clear();
        ScanCount = 0;
        XmlFS.Seek(offset, SeekOrigin.Begin);
        XmlFile = XmlReader.Create(XmlFS);
        int scanNum = -1;
        int indexSet = 0; //1=spectrum,2=chromatogram
        while (XmlFile.Read())
        {
          if (XmlFile.NodeType == XmlNodeType.Element)
          {
            if (XmlFile.Name == "index")
            {
              //TODO: do a better job reading the index
              string tmp = XmlFile.GetAttribute("name");
              if (tmp == "spectrum") indexSet = 1;
              else if (tmp == "chromatogram") indexSet = 2;
              else indexSet = 0;
            }
            else if (XmlFile.Name == "offset")
            {
              if (indexSet == 1) //spectrum offset
              {
                string idRef = XmlFile.GetAttribute("idRef");
                if (idRef != null)
                {
                  scanNum = idRef.IndexOf("scan=");
                  if (scanNum != -1)
                  {
                    scanNum = Convert.ToInt32(idRef.Substring(scanNum + 5));
                    if (scanIndex.Count == 0) FirstScan = firstScanNumber = scanNum;
                    while (scanIndex.Count < scanNum)
                    {
                      scanIndex.Add(0);
                    }
                    ScanCount++;
                  }
                }
                scanIndex.Add(Convert.ToInt32(XmlFile.ReadElementContentAsString()));
              }
              else if (indexSet == 2) //chromatogram offset
              {
                chrIndex.Add(Convert.ToInt32(XmlFile.ReadElementContentAsString()));
              }
            }
          }
          else if (XmlFile.NodeType == XmlNodeType.EndElement)
          {
            if (XmlFile.Name == "indexList") break;
          }
        }

        CurrentScanNumber = 0;
        if (scanNum == -1)
        {
          LastScan = lastScanNumber = scanIndex.Count;
        }
        else
        {
          LastScan = lastScanNumber = scanNum;
        }

        CurrentChromatIndex = -1;
        lastChromatIndex= chrIndex.Count-1;
        ChromatCount = chrIndex.Count;
        //Console.WriteLine(ChromatCount.ToString());

        //Read the last spectrum to get the max retention time
        ParseSpectrum(LastScan);
        MaxRetentionTime = spectrum.RetentionTime;
        Reset();

      }
      catch (Exception ex)
      {
        Console.WriteLine($"Failed to open {ex.Message}");
        return false;
      }
      //Console.WriteLine("Last scan number: " + lastScanNumber.ToString());
      return true;
    }

    /// <summary>
    /// Close the RAW file reader.
    /// </summary>
    public void Close()
    {
      //if (RawFile != null) RawFile.Dispose();
    }

    public Chromatogram GetChromatogram(int chromatIndex = -1)
    {

      if (chromatIndex < 0) CurrentChromatIndex++;
      else CurrentChromatIndex = chromatIndex;
      if (CurrentChromatIndex > lastChromatIndex)
      {
        chromatogram = new Chromatogram(0);
        return chromatogram;
      }

      ParseChromatogram(CurrentChromatIndex);

      return chromatogram;
    }

    public Spectrum GetSpectrum(int scanNumber = -1, bool centroid = true)
    {
      
      if (scanNumber < 0) CurrentScanNumber++;
      else CurrentScanNumber = scanNumber;
      if (CurrentScanNumber > lastScanNumber)
      {
        spectrum = new Spectrum(0);
        return spectrum;
      }

      bool matchScanType = false;
      while (!matchScanType)
      {
        ParseSpectrum(CurrentScanNumber);
        switch (spectrum.MsLevel)
        {
          case 1:
            if (Filter.HasFlag(MSFilter.MS1)) matchScanType = true; break;
          case 2:
            if (Filter.HasFlag(MSFilter.MS2)) matchScanType = true; break;
          case 3:
            if (Filter.HasFlag(MSFilter.MS3)) matchScanType = true; break;
          default: break;
        }

        //We did not match the filter, so advance the scan number and get the next filter.
        if (!matchScanType)
        {
          if (scanNumber < 0)
          {
            CurrentScanNumber++;
            if (CurrentScanNumber > lastScanNumber)
            {
              spectrum = new Spectrum(0);
              return spectrum;
            }
          }
          else //special case where a specific scan number was requested, and did not pass the filter.
          {
            spectrum = new Spectrum(0);
            return spectrum;
          }
        }
      }

      return spectrum;
    }

    public SpectrumEx GetSpectrumEx(int scanNumber = -1, bool centroid = true)
    {
      if (scanNumber < 0) CurrentScanNumber++;
      else CurrentScanNumber = scanNumber;
      if (CurrentScanNumber > lastScanNumber)
      {
        spectrumEx = new SpectrumEx(0);
        return spectrumEx;
      }

      bool matchScanType = false;
      while (!matchScanType)
      {
        ParseSpectrumEx(CurrentScanNumber);
        switch (spectrumEx.MsLevel)
        {
          case 1:
            if (Filter.HasFlag(MSFilter.MS1)) matchScanType = true; break;
          case 2:
            if (Filter.HasFlag(MSFilter.MS2)) matchScanType = true; break;
          case 3:
            if (Filter.HasFlag(MSFilter.MS3)) matchScanType = true; break;
          default: break;
        }

        //We did not match the filter, so advance the scan number and get the next filter.
        if (!matchScanType)
        {
          if (scanNumber < 0)
          {
            CurrentScanNumber++;
            if (CurrentScanNumber > lastScanNumber)
            {
              spectrumEx = new SpectrumEx(0);
              return spectrumEx;
            }
          }
          else //special case where a specific scan number was requested, and did not pass the filter.
          {
            spectrumEx = new SpectrumEx(0);
            return spectrumEx;
          }
        }
      }

      return spectrumEx;
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns>Spectrum object</returns>
    public IEnumerator GetEnumerator()
    {
      Reset();
      Spectrum spec = GetSpectrum();
      while (spec.ScanNumber>0)
      {
        yield return spec;
        spec = GetSpectrum();
      }
      Reset();

      //int FirstScan = firstScanNumber;// RawFile.RunHeaderEx.FirstSpectrum;
      //int LastScan = lastScanNumber;
      //for (int i = FirstScan; i <= LastScan; i++)
      //{
      //  if (i == FirstScan) yield return GetSpectrum(i);
      //  yield return GetSpectrum();
      //}
    }

    private byte[] Decompress(byte[] data, int length)
    {
      byte[] decompressed = new byte[length];
      using (var compressedStream = new MemoryStream(data))
      using (var zipStream = new InflaterInputStream(compressedStream))
      using (var resultStream = new MemoryStream())
      {
        zipStream.CopyTo(resultStream);
        decompressed = resultStream.ToArray();
      }
      return decompressed;
    }

    private void ParseChromatogram(int chromatIndex)
    {

      //Reset spectrum
      chromatogram = new Chromatogram(0);

      //some local variables as we process
      int encLen = 0; //encoded length of a binaryDataArray
      int defArrLen = 0; //default array length, or the number of datapoints in the spectrum.

      XmlFS.Seek(chrIndex[chromatIndex], SeekOrigin.Begin);
      XmlFile = XmlReader.Create(XmlFS);
      while (XmlFile.Read())
      {
        if (XmlFile.NodeType == XmlNodeType.Element)
        {
          if (XmlFile.Name == "binary")
          {
            if(defArrLen>0) ProcessBinaryData(encLen, defArrLen,false,true);
          }
          else if (XmlFile.Name == "binaryDataArray")
          {
            //Set some default assumptions in case these aren't specified in the following cvParams
            bit64 = false;
            zlib = false;
            arrayType = BinaryArrayType.Unknown;
            encLen = Convert.ToInt32(XmlFile.GetAttribute("encodedLength"));
          }
          else if (XmlFile.Name == "cvParam")
          {
            ProcessCvParam(ref XmlFile);
          }
          else if (XmlFile.Name == "chromatogram")
          {
            chromatogram.ID = XmlFile.GetAttribute("id");
            defArrLen = Convert.ToInt32(XmlFile.GetAttribute("defaultArrayLength"));
            chromatogram.Resize(defArrLen);
          }
        }


        else if (XmlFile.NodeType == XmlNodeType.EndElement)
        {
          switch (XmlFile.Name)
          {
            case "chromatogram": return;
            default: break;
          }
        }
      }
    }

    private void ParseSpectrum(int scanNumber)
    {

      //Reset spectrum
      spectrum = new Spectrum(0);  

      hasMonoMz = false;

      //some local variables as we process
      int encLen = 0; //encoded length of a binaryDataArray
      int defArrLen = 0; //default array length, or the number of datapoints in the spectrum.

      XmlFS.Seek(scanIndex[scanNumber], SeekOrigin.Begin);
      XmlFile = XmlReader.Create(XmlFS);
      while (XmlFile.Read())
      {
        if (XmlFile.NodeType == XmlNodeType.Element)
        {
          if (XmlFile.Name == "binary")
          {
            if(defArrLen>0) ProcessBinaryData(encLen, defArrLen);
          }
          else if (XmlFile.Name == "binaryDataArray")
          {
            //Set some default assumptions in case these aren't specified in the following cvParams
            bit64 = false;
            zlib = false;
            arrayType = BinaryArrayType.Unknown;
            encLen = Convert.ToInt32(XmlFile.GetAttribute("encodedLength"));
          }
          else if (XmlFile.Name == "cvParam")
          {
            ProcessCvParam(ref XmlFile);
          }
          else if (XmlFile.Name == "precursor")
          {
            precursorIon.Clear();
            string spectrumRef = XmlFile.GetAttribute("spectrumRef");
            if (spectrumRef != null)
            {
              spectrum.PrecursorMasterScanNumber = Convert.ToInt32(spectrumRef.Substring(spectrumRef.IndexOf("scan=") + 5));
            }
          }
          else if (XmlFile.Name == "spectrum")
          {
            string id = XmlFile.GetAttribute("id");
            spectrum.ScanNumber = Convert.ToInt32(id.Substring(id.IndexOf("scan=") + 5));
            defArrLen = Convert.ToInt32(XmlFile.GetAttribute("defaultArrayLength"));
            spectrum.Resize(defArrLen);
            spectrum.Precursors.Clear();
          }
          else if (XmlFile.Name == "userParam")
          {
            string name = XmlFile.GetAttribute("name");
            string val = XmlFile.GetAttribute("value");
            if (name == "[Thermo Trailer Extra]Monoisotopic M/Z:")
            {
              if (Convert.ToDouble(val) > 1) hasMonoMz = true;
            }
          }
        }


        else if (XmlFile.NodeType == XmlNodeType.EndElement)
        {
          switch (XmlFile.Name)
          {
            case "precursor":
              spectrum.Precursors.Add(precursorIon);
              break;
            case "spectrum": return;
            default: break;
          }
        }
      }
    }

    private void ParseSpectrumEx(int scanNumber)
    {

      //Reset spectrum.
      spectrumEx = new SpectrumEx(0);

      hasMonoMz = false;

      //some local variables as we process
      int encLen = 0; //encoded length of a binaryDataArray
      int defArrLen = 0; //default array length, or the number of datapoints in the spectrum.

      XmlFS.Seek(scanIndex[scanNumber], SeekOrigin.Begin);
      XmlFile = XmlReader.Create(XmlFS);
      while (XmlFile.Read())
      {
        if (XmlFile.NodeType == XmlNodeType.Element)
        {
          if (XmlFile.Name == "binary")
          {
            if(defArrLen>0) ProcessBinaryData(encLen, defArrLen, true);
          }
          else if (XmlFile.Name == "binaryDataArray")
          {
            //Set some default assumptions in case these aren't specified in the following cvParams
            bit64 = false;
            zlib = false;
            arrayType = BinaryArrayType.Unknown;
            encLen = Convert.ToInt32(XmlFile.GetAttribute("encodedLength"));
          }
          else if (XmlFile.Name == "cvParam")
          {
            ProcessCvParam(ref XmlFile, true);
          }
          else if (XmlFile.Name == "precursor")
          {
            precursorIon.Clear();
            string spectrumRef = XmlFile.GetAttribute("spectrumRef");
            if (spectrumRef != null)
            {
              spectrumEx.PrecursorMasterScanNumber = Convert.ToInt32(spectrumRef.Substring(spectrumRef.IndexOf("scan=") + 5));
            }
          }
          else if (XmlFile.Name == "spectrum")
          {
            string id = XmlFile.GetAttribute("id");
            spectrumEx.ScanNumber = Convert.ToInt32(id.Substring(id.IndexOf("scan=") + 5));
            defArrLen = Convert.ToInt32(XmlFile.GetAttribute("defaultArrayLength"));
            spectrumEx.Resize(defArrLen);
            spectrumEx.Precursors.Clear();
          }
          else if (XmlFile.Name == "userParam")
          {
            string name = XmlFile.GetAttribute("name");
            string val = XmlFile.GetAttribute("value");
            if (name == "[Thermo Trailer Extra]Monoisotopic M/Z:")
            {
              if (Convert.ToDouble(val) > 1) hasMonoMz = true;
            }
          }
        }


        else if (XmlFile.NodeType == XmlNodeType.EndElement)
        {
          switch (XmlFile.Name)
          {
            case "precursor":
              spectrumEx.Precursors.Add(precursorIon);
              break;
            case "spectrum": return;
            default: break;
          }
        }
      }
    }

    private void ProcessBinaryData(int encLen, int defArrLen, bool ext = false, bool chromat=false)
    {
      byte[] buffer = new byte[encLen];
      XmlFile.ReadElementContentAsBase64(buffer, 0, encLen);
      int sz = bit64 ? 8 : 4;
      if (zlib)
      {
        buffer = Decompress(buffer, defArrLen * sz);
      }

      if (ext)
      {
        if (arrayType==BinaryArrayType.Mz)
        {
          for (int a = 0; a < defArrLen; a++)
          {
            if (bit64) spectrumEx.DataPoints[a].Mz = BitConverter.ToDouble(buffer, a * sz);
            else spectrumEx.DataPoints[a].Mz = BitConverter.ToSingle(buffer, a * sz);
          }
        }
        else if(arrayType == BinaryArrayType.Intensity)
        {
          for (int a = 0; a < defArrLen; a++)
          {
            if (bit64) spectrumEx.DataPoints[a].Intensity = BitConverter.ToDouble(buffer, a * sz);
            else spectrumEx.DataPoints[a].Intensity = BitConverter.ToSingle(buffer, a * sz);
          }
        }
        else
        {
          //ignoring unknown arrays for now.
        }
      }
      else
      {
        if (arrayType==BinaryArrayType.Time)
        {
          for (int a = 0; a < defArrLen; a++)
          {
            if (bit64) chromatogram.DataPoints[a].RT = BitConverter.ToDouble(buffer, a * sz);
            else chromatogram.DataPoints[a].RT = BitConverter.ToSingle(buffer, a * sz);
          }
        }
        else if (arrayType==BinaryArrayType.Mz)
        {
          for (int a = 0; a < defArrLen; a++)
          {
            if (bit64) spectrum.DataPoints[a].Mz = BitConverter.ToDouble(buffer, a * sz);
            else spectrum.DataPoints[a].Mz = BitConverter.ToSingle(buffer, a * sz);
          }
        }
        else if (arrayType == BinaryArrayType.Intensity)
        {
          if (chromat)
          {
            for (int a = 0; a < defArrLen; a++)
            {
              if (bit64) chromatogram.DataPoints[a].Intensity = BitConverter.ToDouble(buffer, a * sz);
              else chromatogram.DataPoints[a].Intensity = BitConverter.ToSingle(buffer, a * sz);
            }
          }
          else
          {
            for (int a = 0; a < defArrLen; a++)
            {
              if (bit64) spectrum.DataPoints[a].Intensity = BitConverter.ToDouble(buffer, a * sz);
              else spectrum.DataPoints[a].Intensity = BitConverter.ToSingle(buffer, a * sz);
            }
          }
        }
        else
        {
          //ignoring other values
        }
      }
    }

    private void ProcessCvParam(ref XmlReader xml, bool ext = false)
    {
      string acc = xml.GetAttribute("accession");
      string val = xml.GetAttribute("value");
      //TODO: Maybe consider a dictionary, if this becomes burdensome or slow
      //TODO: or not...but considere building the dictionary from an ontology file. Something I'm less happy to do
      //because ontology files are generally overloaded with obsolete terms or simply useless crap.
      switch (acc)
      {
        case "MS:1000016": //scan start time
          double rt = Convert.ToDouble(val);
          string ua = xml.GetAttribute("unitAccession");
          if (ua == "UO:0000030") rt /= 60;
          if (ext) spectrumEx.RetentionTime = rt;
          else spectrum.RetentionTime = rt;
          break;
        case "MS:1000041": //charge state
          precursorIon.Charge = Convert.ToInt32(val);
          break;
        case "MS:1000045": //collision energy
          precursorIon.CollisionEnergy = Convert.ToDouble(val);
          break;
        case "MS:1000127": //centorid spectrum
          if (ext) spectrumEx.Centroid = true;
          else spectrum.Centroid = true;
          break;
        case "MS:1000129": //negative scan
          if (ext) spectrumEx.Polarity = false;
          else spectrum.Polarity = false;
          break;
        case "MS:1000130": //positive scan
          if (ext) spectrumEx.Polarity = true;
          else spectrum.Polarity = true;
          break;
        case "MS:1000133":
          precursorIon.FramentationMethod = FramentationType.CID;
          break;
        case "MS:1000285": //total ion current
          if (ext) spectrumEx.TotalIonCurrent = Convert.ToDouble(val);
          else spectrum.TotalIonCurrent = Convert.ToDouble(val);
          break;
        case "MS:1000421": //high energy collision (obsolete)
        case "MS:1000422": //beam-type collision-induced dissociation
          precursorIon.FramentationMethod = FramentationType.HCD;
          break;
        case "MS:1000500": //scan window upper limit
          if (ext) spectrumEx.EndMz = Convert.ToDouble(val);
          else spectrum.EndMz = Convert.ToDouble(val);
          break;
        case "MS:1000501": //scan window lower limit
          if (ext) spectrumEx.StartMz = Convert.ToDouble(val);
          else spectrum.StartMz = Convert.ToDouble(val);
          break;
        case "MS:1000504": //base peak m/z
          if (ext) spectrumEx.BasePeakMz = Convert.ToDouble(val);
          else spectrum.BasePeakMz = Convert.ToDouble(val);
          break;
        case "MS:1000505": //base peak intensity
          if (ext) spectrumEx.BasePeakIntensity = Convert.ToDouble(val);
          else spectrum.BasePeakIntensity = Convert.ToDouble(val);
          break;
        case "MS:1000511": //ms level
          if (ext) spectrumEx.MsLevel = Convert.ToInt32(val);
          else spectrum.MsLevel = Convert.ToInt32(val);
          break;
        case "MS:1000512": //filter string
          if (ext) spectrumEx.ScanFilter = val;
          else spectrum.ScanFilter = val;

          if (val.Contains("FTMS"))
          {
            if (ext) spectrumEx.Analyzer = "FTMS";
            else spectrum.Analyzer = "FTMS";
          }
          else if (val.Contains("ITMS"))
          {
            if (ext) spectrumEx.Analyzer = "ITMS";
            else spectrum.Analyzer = "OTMS";
          }
          break;
        case "MS:1000514": //m/z array
          arrayType=BinaryArrayType.Mz;
          break;
        case "MS:1000515": //intensity array
          arrayType=BinaryArrayType.Intensity;
          break;
        case "MS:1000521": //32-bit float
          bit64 = false;
          break;
        case "MS:1000523": //64-bit double
          bit64 = true;
          break;
        case "MS:1000527": //highest observed m/z
          if (ext) spectrumEx.HighestMz = Convert.ToDouble(val);
          else spectrum.HighestMz = Convert.ToDouble(val);
          break;
        case "MS:1000528": //lowest observed m/z
          if (ext) spectrumEx.LowestMz = Convert.ToDouble(val);
          else spectrum.LowestMz = Convert.ToDouble(val);
          break;
        case "MS:1000574": //zlib compression
          zlib = true;
          break;
        //Redundant with MS:1000511, and less useful.
        //case "MS:1000579": //MS1 spectrum
        //  spectrum.MsLevel = 1;
        //  break;
        case "MS:1000595": //time array
          arrayType = BinaryArrayType.Time;
          break;

        case "MS:1000598": //electron transfer dissociation
          precursorIon.FramentationMethod = FramentationType.ETD;
          break;
        case "MS:1000599": //pulsed q dissociation
          precursorIon.FramentationMethod = FramentationType.PQD;
          break;
        case "MS:1000744": //selected ion m/z
          //Note that in ProteoWizard mzML files, this value may be set with the IsolationMz if the MonoisotopicMz
          //was not determined. Therefore don't set this unless we know the MonoisotopicMz value is correct.
          if (hasMonoMz) precursorIon.MonoisotopicMz = Convert.ToDouble(val);
          break;
        case "MS:1000827": //isolation window target m/z
          precursorIon.IsolationMz = Convert.ToDouble(val);
          break;
        case "MS:1000828": //isolation window lower offset
          precursorIon.IsolationWidth += Convert.ToDouble(val);
          break;
        case "MS:1000829": //isolation window upper offset
          precursorIon.IsolationWidth += Convert.ToDouble(val);
          break;
        case "MS:1000927": //ion injection time
          if (ext) spectrumEx.IonInjectionTime += Convert.ToDouble(val);
          else spectrum.IonInjectionTime += Convert.ToDouble(val);
          break;

        default: break;
      }
    }


    public void Reset()
    {
      CurrentScanNumber = 0;

    }

  }
}

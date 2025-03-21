using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Buffers.Binary;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Nova.Data;
using ThermoFisher.CommonCore.Data;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System.Collections;
using System.Collections.Specialized;

namespace Nova.Io.Read
{
  internal class MzXMLReader : ISpectrumFileReader
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

    /// <summary>
    /// An enum bitwise operator indicating the desired spectrum levels to read. By default MS1, MS2, and MS3 are read.
    /// </summary>
    private MSFilter Filter { get; set; } = MSFilter.MS1 | MSFilter.MS2 | MSFilter.MS3;

    /// <summary>
    /// The ScanNumber of the last scan in the file.
    /// </summary>
    private int lastScanNumber { get; set; } = 0;
    /// <summary>
    /// The ScanNumber of the most recent scan that was read. A value of 0 means a scan has not yet been read.
    /// </summary>
    private int CurrentScanNumber = 0;

    /// <summary>
    /// Number of bits (64 if true, 32 otherwise) per value in a binary data array
    /// </summary>
    private bool bit64 = false;

    /// <summary>
    /// True if binary data array is zlib compressed.
    /// </summary>
    private bool zlib = false;

    /// <summary>
    /// For storing precursor ion information while parsing the mzML file.
    /// </summary>
    private PrecursorIon precursorIon;

    public int ScanCount { get; private set; } = 0;

    /// <summary>
    /// Constructor for MzXMLReader
    /// </summary>
    /// <param name="filter">The desired scan filter.</param>
    public MzXMLReader(MSFilter filter)
    {
      Filter = filter;
      spectrum = new Spectrum();
      spectrumEx = new SpectrumEx();
      precursorIon = new PrecursorIon();
    }

    /// <summary>
    /// Opens an MzXML file for reading. First reads the entire index, then sets the current position to the beginning of the file and identifying the total number of spectra.
    /// </summary>
    /// <param name="fileName">A valid path to an mzXML file.</param>
    /// <returns>true if file opened successfully, false otherwise.</returns>
    public bool Open(string fileName)
    {
      try
      {
        //Get the offset of the index.
        //TODO: Check to make sure the mzXML is indeed indexed.
        int offset = 0;
        XmlFS = new FileStream(fileName, FileMode.Open, FileAccess.Read);
        byte[] bytes = new byte[200];
        XmlFS.Seek(-200, SeekOrigin.End);
        XmlFS.Read(bytes, offset, 200);
        string block = System.Text.Encoding.Default.GetString(bytes);
        int indexA = block.IndexOf("<indexOffset>");
        int indexB = block.IndexOf("</indexOffset>");
        if(indexA < 0 || indexB < 0)
        {
          throw new Exception("No index found. Please index your mzXML file.");
        }
        offset = int.Parse(block.Substring(indexA + 13, indexB - indexA - 13));

        //read the whole damn index
        scanIndex.Clear();
        XmlFS.Seek(offset, SeekOrigin.Begin);
        XmlFile = XmlReader.Create(XmlFS);
        int scanNum = -1;
        while (XmlFile.Read())
        {
          if (XmlFile.NodeType == XmlNodeType.Element)
          {
            if (XmlFile.Name == "index")
            {
              //TODO: do a better job reading the index
              //Only read the spectrum index. Assumes that it is always first
              if (XmlFile.GetAttribute("name") != "scan") break;
            }
            else if (XmlFile.Name == "offset")
            {
              string idRef = XmlFile.GetAttribute("id");
              if (idRef != null)
              {
                scanNum = Convert.ToInt32(idRef);
                while (scanIndex.Count < scanNum)
                {
                  scanIndex.Add(0);
                }
              }
              scanIndex.Add(Convert.ToInt32(XmlFile.ReadElementContentAsString()));
            }
          }
          else if (XmlFile.NodeType == XmlNodeType.EndElement)
          {
            if (XmlFile.Name == "index") break;
          }
        }

        CurrentScanNumber = 0;
        if (scanNum == -1)
        {
          lastScanNumber = scanIndex.Count;
        }
        else
        {
          lastScanNumber = scanNum;
        }
        ScanCount = scanIndex.Count;
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Failed to open {ex.Message}");
        return false;
      }
      //Console.WriteLine("Last scan number: " + lastScanNumber.ToString());
      return true;
    }

    public void Close()
    {
      //if (RawFile != null) RawFile.Dispose();
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
        ParseSpectrum(CurrentScanNumber, false);
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
        ParseSpectrum(CurrentScanNumber, true);
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
      int FirstScan = 1;// RawFile.RunHeaderEx.FirstSpectrum;
      int LastScan = lastScanNumber;
      for (int i = FirstScan; i <= LastScan; i++)
      {
        yield return GetSpectrum();
      }
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

    private void ParseSpectrum(int scanNumber, bool extended)
    {
      //Reset spectrum. TODO: put this in its own code. Possibly spectrum object
      //Always reset our scan number
      if (extended)
      {
        spectrumEx.ScanNumber = 0;
        spectrumEx.Centroid = false;
        spectrumEx.Polarity = false;
      }
      else
      {
        spectrum.ScanNumber = 0;
        spectrum.Centroid = false;
        spectrum.Polarity = true;
      }

      //some local variables as we process
      int encLen = 0; //encoded length of a binaryDataArray
      int defArrLen = 0; //default array length, or the number of datapoints in the spectrum.

      XmlFS.Seek(scanIndex[scanNumber], SeekOrigin.Begin);
      XmlFile = XmlReader.Create(XmlFS);
      while (XmlFile.Read())
      {
        if (XmlFile.NodeType == XmlNodeType.Element)
        {
          if (XmlFile.Name == "peaks")
          {
            string compressionType = XmlFile.GetAttribute("compressionType");
            string compressedLen = XmlFile.GetAttribute("compressedLen");
            string precision = XmlFile.GetAttribute("precision");
            string byteOrder = XmlFile.GetAttribute("byteOrder");
            string contentType = XmlFile.GetAttribute("contentType");  //not sure what else there is other than m/z-int
            if(precision=="64") bit64 = true;
            else bit64 = false;
            if(compressionType=="zlib") zlib = true;
            else zlib = false;
            encLen = Convert.ToInt32(compressedLen);
            ProcessBinaryData(encLen, defArrLen, extended);
          }
          else if (XmlFile.Name == "precursorMz")
          {
            precursorIon.Clear();
            string precursorScanNum = XmlFile.GetAttribute("precursorScanNum");
            string precursorIntensity = XmlFile.GetAttribute("precursorIntensity");
            string precursorCharge = XmlFile.GetAttribute("precursorCharge");
            string activationMethod = XmlFile.GetAttribute("activationMethod");
            string windowWideness = XmlFile.GetAttribute("windowWideness");
            double mz = XmlFile.ReadElementContentAsDouble();
            if (activationMethod == "HCD") precursorIon.FramentationMethod = FramentationType.HCD;
            else if (activationMethod == "CID") precursorIon.FramentationMethod = FramentationType.CID;
            else if (activationMethod == "ETD") precursorIon.FramentationMethod = FramentationType.ETD;
            if (!precursorCharge.IsNullOrEmpty())
            {
              precursorIon.Charge = Convert.ToInt32(precursorCharge);
              precursorIon.MonoisotopicMz = mz; //this may or may not be the monoisotopicMz
            }
            precursorIon.IsolationMz = mz; //this may or may not be the isolationMz
            precursorIon.Intensity = Convert.ToDouble(precursorIntensity);
            if(!windowWideness.IsNullOrEmpty()) precursorIon.IsolationWidth=Convert.ToDouble(windowWideness);
            if (extended)
            {
              spectrumEx.PrecursorMasterScanNumber=Convert.ToInt32(precursorScanNum);
              spectrumEx.Precursors.Add(precursorIon);
            }
            else
            {
              spectrum.PrecursorMasterScanNumber = Convert.ToInt32(precursorScanNum);
              spectrum.Precursors.Add(precursorIon);
            }
          }
          else if (XmlFile.Name == "scan")
          {
            string num = XmlFile.GetAttribute("num");
            string centroided = XmlFile.GetAttribute("centroided");
            string msLevel = XmlFile.GetAttribute("msLevel");
            string peaksCount = XmlFile.GetAttribute("peaksCount");
            string polarity = XmlFile.GetAttribute("polarity");
            string retentionTime = XmlFile.GetAttribute("retentionTime");
            string lowMz = XmlFile.GetAttribute("lowMz");
            string highMz = XmlFile.GetAttribute("highMz");
            string startMz = XmlFile.GetAttribute("startMz");
            string endMz = XmlFile.GetAttribute("endMz");
            string basePeakMz = XmlFile.GetAttribute("basePeakMz");
            string basePeakIntensity = XmlFile.GetAttribute("basePeakIntensity");
            string totIonCurrent = XmlFile.GetAttribute("totIonCurrent");
            TimeSpan rt=XmlConvert.ToTimeSpan(retentionTime);
            defArrLen = Convert.ToInt32(peaksCount);
            if (extended)
            {
              spectrumEx.ScanNumber = Convert.ToInt32(num);
              if (!centroided.IsNullOrEmpty() && centroided[0] == '1') spectrumEx.Centroid = true;
              spectrumEx.MsLevel = Convert.ToInt32(msLevel);
              spectrumEx.Resize(defArrLen);
              if (!polarity.IsNullOrEmpty() && polarity[0] == '-') spectrumEx.Polarity = false;
              spectrumEx.RetentionTime = rt.TotalMinutes;
              spectrumEx.LowestMz=Convert.ToDouble(lowMz);
              spectrumEx.HighestMz=Convert.ToDouble(highMz);
              spectrumEx.StartMz=Convert.ToDouble(startMz);
              spectrumEx.EndMz=Convert.ToDouble(endMz);
              spectrumEx.BasePeakMz=Convert.ToDouble(basePeakMz);
              spectrumEx.BasePeakIntensity=Convert.ToDouble(basePeakIntensity);
              spectrumEx.TotalIonCurrent=Convert.ToDouble(totIonCurrent);
              spectrumEx.Precursors.Clear();
            }
            else
            {
              spectrum.ScanNumber = Convert.ToInt32(num);
              if (!centroided.IsNullOrEmpty() && centroided[0] == '1') spectrum.Centroid = true;
              spectrum.MsLevel = Convert.ToInt32(msLevel);
              spectrum.Resize(defArrLen);
              if (!polarity.IsNullOrEmpty() && polarity[0] == '-') spectrum.Polarity = false;
              spectrum.RetentionTime = rt.TotalMinutes;
              spectrum.LowestMz = Convert.ToDouble(lowMz);
              spectrum.HighestMz = Convert.ToDouble(highMz);
              spectrum.StartMz = Convert.ToDouble(startMz);
              spectrum.EndMz = Convert.ToDouble(endMz);
              spectrum.BasePeakMz = Convert.ToDouble(basePeakMz);
              spectrum.BasePeakIntensity = Convert.ToDouble(basePeakIntensity);
              spectrum.TotalIonCurrent = Convert.ToDouble(totIonCurrent);
              spectrum.Precursors.Clear();
            }
          }
        }


        else if (XmlFile.NodeType == XmlNodeType.EndElement)
        {
          switch (XmlFile.Name)
          {
            case "precursorMz":
              break;
            case "scan":
              return;
            default: break;
          }
        }
      }
    }

    private void ProcessBinaryData(int encLen, int defArrLen, bool ext = false)
    {
      byte[] buffer = new byte[encLen];
      XmlFile.ReadElementContentAsBase64(buffer, 0, encLen);
      int sz = bit64 ? 8 : 4;
      if (zlib)
      {
        buffer = Decompress(buffer, defArrLen * sz*2);
      }
      int a = 0;
      int pos = 0;
      if (ext)
      {
        while (a < defArrLen)
        {
          if (bit64)
          {
            spectrumEx.DataPoints[a].Mz = ReverseEndianness(BitConverter.ToDouble(buffer, pos));
            pos += sz;
            spectrumEx.DataPoints[a].Intensity = ReverseEndianness(BitConverter.ToDouble(buffer, pos));
            pos += sz;
            a++;
          }
          else
          {
            spectrumEx.DataPoints[a].Mz = ReverseEndianness(BitConverter.ToSingle(buffer, pos));
            pos += sz;
            spectrumEx.DataPoints[a].Intensity = ReverseEndianness(BitConverter.ToSingle(buffer, pos));
            pos += sz;
            a++;
          }
        }
      }
      else
      {
        while (a < defArrLen)
        {
          if (bit64)
          {
            spectrum.DataPoints[a].Mz = ReverseEndianness(BitConverter.ToDouble(buffer, pos));
            pos += sz;
            spectrum.DataPoints[a].Intensity = ReverseEndianness(BitConverter.ToDouble(buffer, pos));
            pos += sz;
            a++;
          }
          else
          {
            spectrum.DataPoints[a].Mz = ReverseEndianness(BitConverter.ToSingle(buffer, pos));
            pos += sz;
            spectrum.DataPoints[a].Intensity = ReverseEndianness(BitConverter.ToSingle(buffer, pos));
            pos += sz;
            a++;
          }
        }
      }
    }

    private double ReverseEndianness(double d)
    {
      Int64 dBits = BitConverter.DoubleToInt64Bits(d);
      Int64 revBits = BinaryPrimitives.ReverseEndianness(dBits);
      return BitConverter.Int64BitsToDouble(revBits);
    }
    private float ReverseEndianness(float f)
    {
      int iBits = BitConverter.SingleToInt32Bits(f);
      int revBits = BinaryPrimitives.ReverseEndianness(iBits);
      return BitConverter.Int32BitsToSingle(revBits);
    }

  }
}

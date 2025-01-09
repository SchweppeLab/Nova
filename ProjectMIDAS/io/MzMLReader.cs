using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ProjectMIDAS.Data.Spectrum;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.FilterEnums;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader;
using ProjectMIDAS.Data;

namespace ProjectMIDAS.Io
{
  internal class MzMLReader : ISpectrumFileReader
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
    private XmlReader XmlFile;

    /// <summary>
    /// The mzML FileStream, which is essential for random access.
    /// </summary>
    private FileStream XmlFS;

    /// <summary>
    /// List of offsets for each spectrum in the mzML file. The position in the index equals the scan number, and a value of zero
    /// indicates the scan number is not in the mzML file.
    /// </summary>
    private List<int> scanIndex = new List<int>();

    /// <summary>
    /// An enum bitwise operator indicating the desired spectrum levels to read. By default MS1, MS2, and MS3 are read.
    /// </summary>
    private MSFilter Filter { get; set; }

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
    private bool bit64= false;

    /// <summary>
    /// True if binary data array is zlib compressed.
    /// </summary>
    private bool zlib = false;

    /// <summary>
    /// True if binary data array is for the m/z values, false for intensity values.
    /// One day there might be other data arrays, so consider using an enum here.
    /// </summary>
    private bool mzArray = false;

    /// <summary>
    /// For storing precursor ion information while parsing the mzML file.
    /// </summary>
    private PrecursorIon precursorIon;

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
        offset = Int32.Parse(block.Substring(indexA + 17, (indexB - indexA - 17)));

        //read the whole damn index
        scanIndex.Clear();
        XmlFS.Seek(offset, SeekOrigin.Begin);
        XmlFile = XmlReader.Create(XmlFS);
        int scanNum = -1;
        while (XmlFile.Read())
        {
          if (XmlFile.NodeType == XmlNodeType.Element)
          {
            if (XmlFile.Name == "index") {
              //TODO: do a better job reading the index
              //Only read the spectrum index. Assumes that it is always first
              if (XmlFile.GetAttribute("name") != "spectrum") break;
            } else if (XmlFile.Name == "offset") {
              string idRef = XmlFile.GetAttribute("idRef");
              if (idRef != null)
              {
                scanNum = idRef.IndexOf("scan=");
                if (scanNum != -1)
                {
                  scanNum = Convert.ToInt32(idRef.Substring(scanNum + 5));
                  while (scanIndex.Count < scanNum)
                  {
                    scanIndex.Add(0);
                  }
                }
              }
              scanIndex.Add(Convert.ToInt32(XmlFile.ReadElementContentAsString()));
            }
          }
          else if (XmlFile.NodeType == XmlNodeType.EndElement)
          {
            if (XmlFile.Name=="indexList") break;
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
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Failed to open {ex.Message}");
        return false;
      }
      Console.WriteLine("Last scan number: " + lastScanNumber.ToString());
      return true;
    }

    /// <summary>
    /// Close the RAW file reader.
    /// </summary>
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

    private void ParseSpectrum(int scanNumber)
    {
      //Always reset our scan number
      spectrum.ScanNumber = 0;

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
            //TODO: Consider wrapping this code in a function to make ParseSpectrum() more readable.
            byte[] buffer = new byte[encLen];
            XmlFile.ReadElementContentAsBase64(buffer,0,encLen);
            int sz = bit64 ? 8 : 4;
            if (zlib) {          
              buffer = Decompress(buffer, defArrLen * sz);
            }
            if (mzArray)
            {
              for(int a = 0; a < defArrLen; a++)
              {
                if (bit64) spectrum.DataPoints[a].Mz = BitConverter.ToDouble(buffer, a * sz);
                else spectrum.DataPoints[a].Mz = BitConverter.ToSingle(buffer, a * sz);
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
          else if(XmlFile.Name == "binaryDataArray")
          {
            //Set some default assumptions in case these aren't specified in the following cvParams
            bit64 = false;
            zlib = false;
            mzArray = true;
            encLen = Convert.ToInt32(XmlFile.GetAttribute("encodedLength"));
          }
          else if(XmlFile.Name == "cvParam")
          {
            ProcessCvParam(ref XmlFile);
          } 
          else if(XmlFile.Name == "precursor")
          {
            precursorIon.Clear();
            string spectrumRef = XmlFile.GetAttribute("spectrumRef");
            spectrum.PrecursorMasterScanNumber = Convert.ToInt32(spectrumRef.Substring(spectrumRef.IndexOf("scan=") + 5));
          }
          else if (XmlFile.Name == "spectrum")
          {
            string id = XmlFile.GetAttribute("id");
            spectrum.ScanNumber = Convert.ToInt32(id.Substring(id.IndexOf("scan=") + 5));
            defArrLen = Convert.ToInt32(XmlFile.GetAttribute("defaultArrayLength"));
            spectrum.Resize(defArrLen);
            spectrum.Precursors.Clear();
          } 
        } 
        

        else if(XmlFile.NodeType == XmlNodeType.EndElement)
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

    private void ProcessBinaryData(string dat)
    {

    }

    private void ProcessCvParam(ref XmlReader xml)
    {
      string acc = xml.GetAttribute("accession");
      string val = xml.GetAttribute("value");
      switch (acc)
      {
        case "MS:1000016": //scan start time
          spectrum.RetentionTime=Convert.ToDouble(val);
          string ua = xml.GetAttribute("unitAccession");
          if (ua == "UO:0000030") spectrum.RetentionTime /= 60;
          break;
        case "MS:1000041": //charge state
          precursorIon.Charge = Convert.ToInt32(val);
          break;
        case "MS:1000511": //ms level
          spectrum.MsLevel = Convert.ToInt32(val);
          break;
        case "MS:1000512": //filter string
          spectrum.ScanFilter = val;
          break;
        case "MS:1000514": //m/z array
          mzArray = true;
          break;
        case "MS:1000515": //intensity array
          mzArray = false;
          break;
        case "MS:1000521": //32-bit float
          bit64 = false;
          break;
        case "MS:1000523": //64-bit double
          bit64 = true;
          break;
        case "MS:1000574": //zlib compression
          zlib = true;
          break;
        case "MS:1000744": //selected ion m/z
          precursorIon.MonoisotopicMz = Convert.ToDouble(val);
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

        default: break;
      }
    }
  }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Http.HttpResults;
using Newtonsoft.Json.Linq;
using Nova.Data;
using ThermoFisher.CommonCore.Data;
using ThermoFisher.CommonCore.Data.Business;

namespace Nova.Io.Read
{
  //TODO: Seriously rethink supporting this format, since it allows for conflicting meta-information conventions.
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
    /// The mzML FileStream, which is essential for random access.
    /// </summary>
    private FileStream? FS;

    private StreamReader? SR;

    /// <summary>
    /// An enum bitwise operator indicating the desired spectrum levels to read. By default MS1, MS2, and MS3 are read.
    /// </summary>
    public MSFilter Filter { get; set; } = MSFilter.MS1 | MSFilter.MS2 | MSFilter.MS3;

    /// <summary>
    /// The ScanNumber of the last scan in the file.
    /// </summary>
    private int lastScanNumber { get; set; } = 0;
    /// <summary>
    /// The ScanNumber of the most recent scan that was read. A value of 0 means a scan has not yet been read.
    /// </summary>
    private int CurrentScanNumber = 0;

    /// <summary>
    /// For storing precursor ion information while parsing the mzML file.
    /// </summary>
    private PrecursorIon precursorIon;

    public int ScanCount { get; private set; } = 0;

    /// <summary>
    /// Constructor for MGFReader
    /// </summary>
    /// <param name="filter">The desired scan filter.</param>
    public MGFReader(MSFilter filter)
    {
      Filter = filter;
      spectrum = new Spectrum();
      spectrumEx = new SpectrumEx();
      precursorIon = new PrecursorIon();
    }

    /// <summary>
    /// Opens an MGF file for reading.
    /// </summary>
    /// <param name="fileName">A valid path to an MGF file.</param>
    /// <returns>true if file opened successfully, false otherwise.</returns>
    public bool Open(string fileName)
    {
      try
      {
        FS = new FileStream(fileName, FileMode.Open, FileAccess.Read);
        SR = new StreamReader(FS);
        bool endOfHeader = false;
        bool monoisotopic = false;
        List<int> charges = new List<int>();

        //Read global header information
        while (!SR.EndOfStream)
        {
          string line = SR.ReadLine();
          if (line.IsNullOrEmpty()) continue;
          if (line[0] == '#' || line[0] == ';' || line[0] == '!' || line[0] == '/') continue; //skip comment lines

          string[] tokens = line.Split("=\n\r");
          switch (tokens[0])
          {
            case "BEGIN IONS":
              endOfHeader = true;
              break;
            case "CHARGE":
              string[] z = tokens[1].Split(" \t\n\r");
              foreach (string s in z)
              {
                //skip anything that doesn't start with a number
                if (!Char.IsNumber(s[0])) continue;

                bool neg = false;
                string num = string.Empty;
                foreach (char c in s)
                {
                  if (c == '-') neg = true;
                  if (Char.IsDigit(c)) num += c;
                }
                if (neg) charges.Add(-Convert.ToInt32(num));
                else charges.Add(Convert.ToInt32(num));
              }
              break;
            case "MASS":
              if (tokens[1] == "Monoisotopic") monoisotopic = true;
              break;
            default:
              if (tokens[0][0] == '_')
              {
                //user and reserved parameters here. Like _DISTILLER_RAWFILE
              }
              break;

          }
        }
        if (!SR.EndOfStream) return false;

        //CurrentScanNumber = 0;
        //if (scanNum == -1)
        //{
        //  lastScanNumber = scanIndex.Count;
        //}
        //else
        //{
        //  lastScanNumber = scanNum;
        //}
        //ScanCount = scanIndex.Count;
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
      return new Spectrum(0);
    }

    public SpectrumEx GetSpectrumEx(int scanNumber = -1, bool centroid = true)
    {
      return new SpectrumEx(0);
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

    public void Reset()
    {

    }

  }
}

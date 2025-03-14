using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nova.Data;
using ThermoFisher.CommonCore.Data;
using ThermoFisher.CommonCore.RawFileReader;

namespace Nova.Io
{

  public enum FileFormat
  {
    Unknown,    //Default format until otherwise determined.
    MzML,
    MzXML,
    ThermoRaw,
  };

  /// <summary>
  /// Bitwise enumerator for filtering scans by type.
  /// </summary>
  [Flags]
  public enum MSFilter
  {
    None = 0,
    MS1=1,
    MS2=2,
    MS3=4
  }

  public class FileReader
  {

    /// <summary>
    /// Identifies the format of the most recently opened file.
    /// </summary>
    public FileFormat Format {  get; } = FileFormat.Unknown;
    private ISpectrumFileReader? fileReader { get; set; }

    /// <summary>
    /// Filters out scans when reading scan-by-scan. For example, to read only MS2 scans, set filter=MSFilter.MS2. By default, all 
    /// supported scan levels are set (Filter=MSFilter.MS1 | MSFilter.MS2 | MSFilter.MS3).
    /// </summary>
    public MSFilter Filter { get; set; } = MSFilter.MS1 | MSFilter.MS2 | MSFilter.MS3;

    /// <summary>
    /// The name of the file currently being read.
    /// </summary>
    public string FileName { get; set; } = "";


    /// <summary>
    /// Checks to see if we requested, or already have, a valid file from which to read a spectrum. Use null to specify reading
    /// another spectrum from the same file.
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns>true if the file is already open, false if not open</returns>
    /// <exception cref="ArgumentNullException">No file name was given and no file is open.</exception>
    /// <exception cref="FileNotFoundException">File not found.</exception>
    public bool CheckFile(string fileName)
    {
      //first, check if we're requesting a new spectrum from the same file
      if (fileName.IsNullOrEmpty())
      {
        if (FileName.IsNullOrEmpty())
        {
          throw new ArgumentNullException(fileName, "empty file name invalid unless file has already been opened.");
        }
        return true;
      }
      
      //next, check if we're reading the same file
      if (fileName == FileName) return true;

      //finally, check if this file exists
      if(!File.Exists(fileName)) 
      {
        throw new FileNotFoundException("file not found", fileName);
      }
      return false;
    }

    /// <summary>
    /// Reads a file name string and returns the FileFormat value based on the file extension characters. FormatException thrown
    /// if file doesn't have an exception or the extension isn't recognized.
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public FileFormat CheckFileFormat(string fileName)
    {
      string ext = Path.GetExtension(fileName);
      if(ext == null) throw new FormatException("file extension required.");
      else 
      {
        ext = ext.ToLower();
        if (ext == ".raw") return FileFormat.ThermoRaw;
        if (ext == ".mzml") return FileFormat.MzML;
        if (ext == ".mzxml") return FileFormat.MzXML;
      }
      throw new FormatException(ext + " not recognized.");
    }

    public bool OpenSpectrumFile(string fileName)
    {
      //Check file extension to determine file type.
      FileFormat ff = CheckFileFormat(fileName);
      switch (ff)
      {
        case FileFormat.ThermoRaw:
          fileReader = new ThermoRawReader(Filter);
          break;

        case FileFormat.MzML:
          fileReader = new MzMLReader(Filter);
          break;

        case FileFormat.Unknown:
          return false;
      }
      FileName = fileName;
      fileReader.Open(fileName);
      return true;
    }

    public Spectrum ReadSpectrum(string fileName="", int scanNumber = -1, bool centroid = true)
    {
      try
      {

        //If CheckFile returns false, that means we need to open a new file
        if (!CheckFile(fileName))
        {
          //close the existing file, if open
          if (!FileName.IsNullOrEmpty())
          {
            FileName = String.Empty;
            fileReader.Close();
          }

          //Open the new file and set the FileName
          OpenSpectrumFile(fileName);
        }

        //Try to read the spectrum
        try
        {
          return fileReader.GetSpectrum(scanNumber, centroid);
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.Message);
          return new Spectrum(0);
        }

      }
      catch (Exception ex) 
      {
        //TODO: Handle file checking exceptions
        Console.WriteLine(ex.ToString());
      }

      return new Spectrum(0);
    }

    public void Reset()
    {
      FileName = String.Empty;
      fileReader.Close();
    }

  }
}

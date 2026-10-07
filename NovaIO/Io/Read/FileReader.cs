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

using System.Collections;
using Nova.Data;

namespace Nova.Io.Read
{

  /// <summary>
  /// The file formats <see cref="FileReader"/> can open
  /// </summary>
  public enum FileFormat
  {
    Unknown,    //Default format until otherwise determined.
    MGF,
    MzML,
    MzXML,
    ThermoRaw,
  }

  /// <summary>
  /// Bitwise enumerator for filtering scans by type.
  /// </summary>
  [Flags]
  public enum MSFilter
  {
    None = 0,
    MS1 = 1,
    MS2 = 2,
    MS3 = 4
  }

  /// <summary>
  /// Reads spectra and chromatograms from an MS data file, choosing the reader by file extension. Enumerating it yields each
  /// spectrum in file order
  /// </summary>
  public class FileReader : IEnumerable
  {

    /// <summary>
    /// Identifies the format of the most recently opened file.
    /// </summary>
    public FileFormat Format { get; private set; } = FileFormat.Unknown;
    private ISpectrumFileReader? fileReader { get; set; }

    /// <summary>
    /// Filters out scans when reading scan-by-scan. For example, to read only MS2 scans, set filter=MSFilter.MS2. By default, all 
    /// supported scan levels are set (Filter=MSFilter.MS1 | MSFilter.MS2 | MSFilter.MS3).
    /// </summary>
    private MSFilter Filter { get; set; } = MSFilter.MS1 | MSFilter.MS2 | MSFilter.MS3;

    /// <summary>
    /// The name of the file currently being read.
    /// </summary>
    public string FileName { get; set; } = "";

    /// <summary>
    /// The number of spectra in the open file
    /// </summary>
    public int ScanCount { get; private set; } = 0;

    /// <summary>
    /// Number of chromatograms in the open file, or 0 for formats that carry none.
    /// </summary>
    public int ChromatCount { get; private set; } = 0;

    /// <summary>
    /// The scan number of the first spectrum in the file
    /// </summary>
    public int FirstScan { get; private set; } = 0;

    /// <summary>
    /// The scan number of the last spectrum in the file
    /// </summary>
    public int LastScan { get; private set; } = 0;

    /// <summary>
    /// The retention time at the end of the run, in minutes
    /// </summary>
    public double MaxRetentionTime { get; private set; } = 0;

    /// <summary>
    /// Creates a reader with no file open
    /// </summary>
    /// <param name="filter">The MS levels to read</param>
    public FileReader (MSFilter filter = MSFilter.MS1 | MSFilter.MS2 | MSFilter.MS3)
    {
      Filter = filter;
    }

    /// <summary>
    /// Creates a reader and opens the file
    /// </summary>
    /// <param name="filename">The file to open</param>
    /// <param name="filter">The MS levels to read</param>
    /// <exception cref="FormatException">The file extension is not a recognized format</exception>
    /// <exception cref="FileNotFoundException">The file could not be opened</exception>
    public FileReader(string filename,MSFilter filter = MSFilter.MS1 | MSFilter.MS2 | MSFilter.MS3)
    {
      Filter = filter;
      if (!OpenSpectrumFile(filename))
      {
        throw new FileNotFoundException(filename);
      }
    }

    /// <summary>
    /// Checks to see if we requested, or already have, a valid file from which to read a spectrum. Use null to specify reading
    /// another spectrum from the same file.
    /// </summary>
    /// <param name="fileName">The file to read from, or empty to keep reading the current file</param>
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
      if (!File.Exists(fileName))
      {
        throw new FileNotFoundException("file not found", fileName);
      }
      return false;
    }

    /// <summary>
    /// Reads a file name string and returns the FileFormat value based on the file extension characters. FormatException thrown
    /// if file doesn't have an extension or the extension isn't recognized.
    /// </summary>
    /// <param name="fileName">The file name; only its extension is examined</param>
    /// <returns>The format matching the extension</returns>
    public static FileFormat CheckFileFormat(string fileName)
    {
      string ext = Path.GetExtension(fileName);
      if (ext == null) throw new FormatException("file extension required.");
      else
      {
        ext = ext.ToLower();
        if (ext == ".raw") return FileFormat.ThermoRaw;
        if (ext == ".mzml") return FileFormat.MzML;
        if (ext == ".mzxml") return FileFormat.MzXML;
        if (ext == ".mgf") return FileFormat.MGF;
      }
      throw new FormatException(ext + " not recognized.");
    }

    /// <summary>
    /// Constructs an unopened reader for the given format, or null if no reader is available
    /// for it (e.g. FileFormat.Unknown). Shared by OpenSpectrumFile and
    /// SpectrumFileReaderFactory.GetReader so extension-to-reader mapping has one source of
    /// truth.
    /// </summary>
    internal static ISpectrumFileReader? CreateReader(FileFormat format, MSFilter filter)
    {
      switch (format)
      {
        case FileFormat.ThermoRaw: return new ThermoRawReader(filter);
        case FileFormat.MzML: return new MzMLReader(filter);
        case FileFormat.MzXML: return new MzXMLReader(filter);
        case FileFormat.MGF: return new MGFReader(filter);
        default: return null;
      }
    }

    /// <summary>
    /// Yields each spectrum in the open file in file order, subject to the current filter, then resets the reader
    /// </summary>
    /// <returns>An enumerator over <see cref="Spectrum"/> objects</returns>
    public IEnumerator GetEnumerator()
    {
      fileReader.Reset();
      Spectrum spec = fileReader.GetSpectrum();
      while (spec.ScanNumber > 0)
      {
        yield return spec;
        spec = fileReader.GetSpectrum();
      }
      fileReader.Reset();
      //int FirstScan = fileReader.FirstScan;// RawFile.RunHeaderEx.FirstSpectrum;
      //int LastScan = fileReader.LastScan;
      //for (int i = FirstScan; i <= LastScan; i++)
      //{
      //  if (i == FirstScan) yield return fileReader.GetSpectrum(i);
      //  yield return fileReader.GetSpectrum();
      //}
    }

    /// <summary>
    /// Opens a file for reading, closing any file already open. The reader is chosen by file extension
    /// </summary>
    /// <param name="fileName">The file to open</param>
    /// <returns>true if the file opened and is ready to read</returns>
    /// <exception cref="FormatException">The file extension is not a recognized format</exception>
    public bool OpenSpectrumFile(string fileName)
    {
      fileReader?.Close();
      //Check file extension to determine file type.
      FileFormat ff = CheckFileFormat(fileName);
      if (ff == FileFormat.Unknown) return false;

      //CreateReader only returns null for FileFormat.Unknown, already handled above, so this
      //is never actually null for any format CheckFileFormat can produce -- kept nullable and
      //guarded anyway since CreateReader is a general format-to-reader mapping, not something
      //that should assume every caller pre-filters Unknown the way this one does.
      ISpectrumFileReader? reader = CreateReader(ff, Filter);
      if (reader != null) fileReader = reader;

      Format = ff;
      FileName = fileName;
      bool opened = fileReader.Open(fileName);
      ScanCount = fileReader.ScanCount;
      ChromatCount = fileReader.ChromatCount;
      FirstScan = fileReader.FirstScan;
      LastScan = fileReader.LastScan;
      MaxRetentionTime = fileReader.MaxRetentionTime;
      return opened;
    }

    /// <summary>
    /// Reads a chromatogram, opening the file first if it is not the current one
    /// </summary>
    /// <param name="fileName">The file to read from, or empty for the current file</param>
    /// <param name="chromatIndex">The chromatogram index (zero based), or -1 for the next one</param>
    /// <returns>The chromatogram, or an empty one if it could not be read</returns>
    /// <exception cref="SpectrumFileOpenException">A new file could not be opened</exception>
    public Chromatogram ReadChromatogram(string fileName = "", int chromatIndex = -1)
    {
      try
      {

        //If CheckFile returns false, that means we need to open a new file
        if (!CheckFile(fileName))
        {
          //close the existing file, if open
          if (!FileName.IsNullOrEmpty())
          {
            FileName = string.Empty;
            fileReader.Close();
          }

          //Open the new file and set the FileName
          if (!OpenSpectrumFile(fileName))
          {
            string detail = (fileReader as IOpenFailureDetail)?.OpenFailure ?? "the reader reported failure without detail.";
            throw new SpectrumFileOpenException(fileName, detail);
          }
        }

        //Try to read the spectrum
        try
        {
          return fileReader.GetChromatogram(chromatIndex);
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.Message);
          return new Chromatogram(0);
        }

      }
      catch (SpectrumFileOpenException)
      {
        //Narrow on purpose: catching IOException would also propagate the FileNotFoundException CheckFile can raise
        throw;
      }
      catch (Exception ex)
      {
        //TODO: Handle file checking exceptions
        Console.WriteLine(ex.ToString());
      }

      return new Chromatogram(0);
    }

    /// <summary>
    /// Reads a spectrum, opening the file first if it is not the current one
    /// </summary>
    /// <param name="fileName">The file to read from, or empty for the current file</param>
    /// <param name="scanNumber">The scan number, or -1 for the next spectrum</param>
    /// <param name="centroid">The preferred peak type. Not guaranteed; check the Spectrum.Centroid property</param>
    /// <returns>The spectrum, or an empty one if it could not be read</returns>
    /// <exception cref="SpectrumFileOpenException">A new file could not be opened</exception>
    public Spectrum ReadSpectrum(string fileName = "", int scanNumber = -1, bool centroid = true)
    {
      try
      {

        //If CheckFile returns false, that means we need to open a new file
        if (!CheckFile(fileName))
        {
          //close the existing file, if open
          if (!FileName.IsNullOrEmpty())
          {
            FileName = string.Empty;
            fileReader.Close();
          }

          //Open the new file and set the FileName
          if (!OpenSpectrumFile(fileName))
          {
            string detail = (fileReader as IOpenFailureDetail)?.OpenFailure ?? "the reader reported failure without detail.";
            throw new SpectrumFileOpenException(fileName, detail);
          }
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
      catch (SpectrumFileOpenException)
      {
        //Narrow on purpose: catching IOException would also propagate the FileNotFoundException CheckFile can raise
        throw;
      }
      catch (Exception ex)
      {
        //TODO: Handle file checking exceptions
        Console.WriteLine(ex.ToString());
      }

      return new Spectrum(0);
    }

    /// <summary>
    /// Reads a spectrum with the extended per-peak data where the format provides it, opening the file first if it is not the
    /// current one
    /// </summary>
    /// <param name="fileName">The file to read from, or empty for the current file</param>
    /// <param name="scanNumber">The scan number, or -1 for the next spectrum</param>
    /// <param name="centroid">The preferred peak type. Not guaranteed; check the Spectrum.Centroid property</param>
    /// <returns>The spectrum, or an empty one if it could not be read</returns>
    /// <exception cref="SpectrumFileOpenException">A new file could not be opened</exception>
    public SpectrumEx ReadSpectrumEx(string fileName = "", int scanNumber = -1, bool centroid = true)
    {
      try
      {

        //If CheckFile returns false, that means we need to open a new file
        if (!CheckFile(fileName))
        {
          //close the existing file, if open
          if (!FileName.IsNullOrEmpty())
          {
            FileName = string.Empty;
            fileReader.Close();
          }

          //Open the new file and set the FileName
          if (!OpenSpectrumFile(fileName))
          {
            string detail = (fileReader as IOpenFailureDetail)?.OpenFailure ?? "the reader reported failure without detail.";
            throw new SpectrumFileOpenException(fileName, detail);
          }
        }

        //Try to read the spectrum
        try
        {
          return fileReader.GetSpectrumEx(scanNumber, centroid);
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.Message);
          return new SpectrumEx(0);
        }

      }
      catch (SpectrumFileOpenException)
      {
        //Narrow on purpose: catching IOException would also propagate the FileNotFoundException CheckFile can raise
        throw;
      }
      catch (Exception ex)
      {
        //TODO: Handle file checking exceptions
        Console.WriteLine(ex.ToString());
      }

      return new SpectrumEx(0);
    }

    /// <summary>
    /// Returns to the start of the open file for sequential reading
    /// </summary>
    public void Reset()
    {
      fileReader?.Reset();
      //FileName = string.Empty;
      //fileReader.Close();
    }

    /// <summary>
    /// Sets the MS levels to read, for this reader and any file it has open
    /// </summary>
    /// <param name="filter">The MS levels to read</param>
    public void SetFilter(MSFilter filter)
    {
      Filter = filter;
      if (fileReader != null) fileReader.Filter = filter;
    }

  }
}

using System.Collections;

using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.FilterEnums;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader;

using Nova.Data;
using Nova.Io.Meta;


namespace Nova.Io.Read
{
  public class ThermoRawReader : ISpectrumFileReader
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
    /// The interface to the Raw file.
    /// </summary>
    private IRawDataExtended? RawFile;

    /// <summary>
    /// An enum bitwise operator indicating the desired spectrum levels to read. By default MS1, MS2, and MS3 are read.
    /// </summary>
    public MSFilter Filter { get; set; } = MSFilter.MS1 | MSFilter.MS2 | MSFilter.MS3;

    /// <summary>
    /// The ScanNumber of the most recent scan that was read. A value of 0 means a scan has not yet been read.
    /// </summary>
    private int CurrentScanNumber = 0;

    #region Inherited interface properties.
    public int FirstScanNumber { get; private set; } = 0;

    public int LastScanNumber { get; private set; } = 0;

    public double MaxRetentionTime { get; private set; } = 0;

    public int ScanCount { get; private set; } = 0;
    #endregion

    /// <summary>
    /// Constructor for ThermoRawReader
    /// </summary>
    /// <param name="filter">The desired scan filter.</param>
    public ThermoRawReader(MSFilter filter)
    {
      Filter = filter;
      spectrum = new Spectrum();
      spectrumEx = new SpectrumEx();
    }

    /// <summary>
    /// Enumerator interface for the ThermoRawReader class. Scan filter by MS level still applies.
    /// </summary>
    /// <returns>Spectrum object</returns>
    public IEnumerator GetEnumerator()
    {
      int FirstScan = RawFile.RunHeaderEx.FirstSpectrum;
      int LastScan = RawFile.RunHeaderEx.LastSpectrum;
      for (int i = FirstScan; i <= LastScan; i++)
      {
        if (i == FirstScan) yield return GetSpectrum(i);
        yield return GetSpectrum();
      }
    }

    /// <summary>
    /// Returns the requested spectrum from the file. If the desired spectrum could not be read, then an empty spectrum with
    /// a ScanNumber of 0 is returned.
    /// </summary>
    /// <param name="scanNumber">The desired scan number, or -1 to get the next scan in the file.</param>
    /// <param name="centroid">Request centroid data; if centroid data can not be obtained, profile data is returned.</param>
    /// <returns>Spectrum object</returns>
    public Spectrum GetSpectrum(int scanNumber = -1, bool centroid = true)
    {
      //Set the scan number, or if one wasn't specified (i.e. -1), then go to the next scan.
      if (scanNumber < 0) CurrentScanNumber++;
      else CurrentScanNumber = scanNumber;
      if (CurrentScanNumber > LastScanNumber)
      {
        spectrum = new Spectrum(0);
        return spectrum;
      }

      //Check the scan type
      IScanFilter scanFilter = RawFile.GetFilterForScanNumber(CurrentScanNumber);
      bool matchScanType = false;
      while (!matchScanType)
      {

        //Check the scan type against our filter
        switch (scanFilter.MSOrder)
        {
          case MSOrderType.Ms:
            if (Filter.HasFlag(MSFilter.MS1)) matchScanType = true; break;
          case MSOrderType.Ms2:
            if (Filter.HasFlag(MSFilter.MS2)) matchScanType = true; break;
          case MSOrderType.Ms3:
            if (Filter.HasFlag(MSFilter.MS3)) matchScanType = true; break;
          default: break;
        }

        //We did not match the filter, so advance the scan number and get the next filter.
        if (!matchScanType)
        {
          if (scanNumber < 0)
          {
            CurrentScanNumber++;
            if (CurrentScanNumber > LastScanNumber)
            {
              spectrum = new Spectrum(0);
              return spectrum;
            }
            scanFilter = RawFile.GetFilterForScanNumber(CurrentScanNumber);
          }
          else //special case where a specific scan number was requested, and did not pass the filter.
          {
            spectrum = new Spectrum(0);
            return spectrum;
          }
        }

      }

      ScanStatistics scanStatistics = RawFile.GetScanStatsForScanNumber(CurrentScanNumber);
      //double retentionTime = RawFile.RetentionTimeFromScanNumber(CurrentScanNumber);

      //check scan type. If profile, and user wants centroid, see if centroid version is also available.
      if (centroid)
      {
        //Regardless of actual scan peak type, see if it contains centroid peak data.
        bool centroidOK = false;
        if (scanStatistics.IsCentroidScan) centroidOK = true;
        else if (scanStatistics.SpectrumPacketType == SpectrumPacketType.FtProfile) centroidOK = true;

        //Get the centroid peaks
        if (centroidOK)
        {
          var centroidStream = RawFile.GetCentroidStream(CurrentScanNumber, false);
          if (centroidStream.Length > 0)
          {
            spectrum = new Spectrum(centroidStream.Length);
            for (int i = 0; i < centroidStream.Length; i++)
            {
              spectrum.DataPoints[i].Mz = centroidStream.Masses[i];
              spectrum.DataPoints[i].Intensity = centroidStream.Intensities[i];
            }
            ProcessSpectrumInformation(scanFilter, scanStatistics);
            return spectrum;
          }
        }

      }

      //if we got here, it is because profile data is preferred
      var segmentedScan = RawFile.GetSegmentedScanFromScanNumber(CurrentScanNumber, scanStatistics);
      spectrum = new Spectrum(segmentedScan.Positions.Length);
      for (int i = 0; i < segmentedScan.Positions.Length; i++)
      {
        spectrum.DataPoints[i].Mz = segmentedScan.Positions[i];
        spectrum.DataPoints[i].Intensity = segmentedScan.Intensities[i];
      }
      ProcessSpectrumInformation(scanFilter, scanStatistics);
      return spectrum;
    }

    /// <summary>
    /// Returns the requested extended spectrum from the file. If the desired spectrum could not be read, then an empty spectrum with
    /// a ScanNumber of 0 is returned. 
    /// TODO: Finish this function; it currently only returns an empty extended spectrum.
    /// </summary>
    /// <param name="scanNumber">The desired scan number, or -1 to get the next scan in the file.</param>
    /// <param name="centroid">Request centroid data; if centroid data can not be obtained, profile data is returned.</param>
    /// <returns>SpectrumEx object</returns>
    public SpectrumEx GetSpectrumEx(int scanNumber = -1, bool centroid = true)
    {
      //Set the scan number, or if one wasn't specified (i.e. -1), then go to the next scan.
      if (scanNumber < 0) CurrentScanNumber++;
      else CurrentScanNumber = scanNumber;
      if (CurrentScanNumber > LastScanNumber)
      {
        spectrumEx = new SpectrumEx(0);
        return spectrumEx;
      }

      //Check the scan type
      IScanFilter scanFilter = RawFile.GetFilterForScanNumber(CurrentScanNumber);
      bool matchScanType = false;
      while (!matchScanType)
      {

        //Check the scan type against our filter
        switch (scanFilter.MSOrder)
        {
          case MSOrderType.Ms:
            if (Filter.HasFlag(MSFilter.MS1)) matchScanType = true; break;
          case MSOrderType.Ms2:
            if (Filter.HasFlag(MSFilter.MS2)) matchScanType = true; break;
          case MSOrderType.Ms3:
            if (Filter.HasFlag(MSFilter.MS3)) matchScanType = true; break;
          default: break;
        }

        //We did not match the filter, so advance the scan number and get the next filter.
        if (!matchScanType)
        {
          if (scanNumber < 0)
          {
            CurrentScanNumber++;
            if (CurrentScanNumber > LastScanNumber)
            {
              spectrumEx = new SpectrumEx(0);
              return spectrumEx;
            }
            scanFilter = RawFile.GetFilterForScanNumber(CurrentScanNumber);
          }
          else //special case where a specific scan number was requested, and did not pass the filter.
          {
            spectrumEx = new SpectrumEx(0);
            return spectrumEx;
          }
        }

      }

      ScanStatistics scanStatistics = RawFile.GetScanStatsForScanNumber(CurrentScanNumber);
      //double retentionTime = RawFile.RetentionTimeFromScanNumber(CurrentScanNumber);

      //check scan type. If profile, and user wants centroid, see if centroid version is also available.
      if (centroid)
      {
        //Regardless of actual scan peak type, see if it contains centroid peak data.
        bool centroidOK = false;
        if (scanStatistics.IsCentroidScan) centroidOK = true;
        else if (scanStatistics.SpectrumPacketType == SpectrumPacketType.FtProfile) centroidOK = true;

        //Get the centroid peaks
        if (centroidOK)
        {
          var centroidStream = RawFile.GetCentroidStream(CurrentScanNumber, false);
          if (centroidStream.Length > 0)
          {
            spectrumEx = new SpectrumEx(centroidStream.Length);
            for (int i = 0; i < centroidStream.Length; i++)
            {
              spectrumEx.DataPoints[i].Mz = centroidStream.Masses[i];
              spectrumEx.DataPoints[i].Intensity = centroidStream.Intensities[i];
              spectrumEx.DataPoints[i].Resolution = centroidStream.Resolutions[i];
              spectrumEx.DataPoints[i].Noise = centroidStream.Noises[i];
              spectrumEx.DataPoints[i].Charge = Convert.ToInt32(centroidStream.Charges[i]);
            }
            ProcessSpectrumInformation(scanFilter, scanStatistics, true);
            return spectrumEx;
          }
        }

      }

      //if we got here, it is because profile data is preferred
      var segmentedScan = RawFile.GetSegmentedScanFromScanNumber(CurrentScanNumber, scanStatistics);
      spectrumEx = new SpectrumEx(segmentedScan.Positions.Length);
      for (int i = 0; i < segmentedScan.Positions.Length; i++)
      {
        spectrumEx.DataPoints[i].Mz = segmentedScan.Positions[i];
        spectrumEx.DataPoints[i].Intensity = segmentedScan.Intensities[i];
      }
      ProcessSpectrumInformation(scanFilter, scanStatistics, true);
      return spectrumEx;
    }

    /// <summary>
    /// Opens a Thermo RAW file for reading, setting the current position to the beginning of the file and identifying the total number of spectra.
    /// </summary>
    /// <param name="fileName">A valid path to a Thermo RAW file.</param>
    /// <returns>true if file opened successfully, false otherwise.</returns>
    public bool Open(string fileName)
    {
      RawFile = RawFileReaderAdapter.FileFactory(fileName);
      if (!RawFile.IsOpen) return false;
      RawFile.SelectInstrument(Device.MS, 1);
      CurrentScanNumber = 0;
      FirstScanNumber = RawFile.RunHeaderEx.FirstSpectrum;
      LastScanNumber = RawFile.RunHeaderEx.LastSpectrum;
      ScanCount = RawFile.RunHeaderEx.SpectraCount;
      MaxRetentionTime = RawFile.RetentionTimeFromScanNumber(LastScanNumber);
      return true;
    }

    /// <summary>
    /// Close the RAW file reader.
    /// </summary>
    public void Close()
    {
      if (RawFile != null) RawFile.Dispose();
    }

    /// <summary>
    /// A single function call to simplify gathering spectrum information from various sources.
    /// </summary>
    /// <param name="scanFilter">Optionally provide an IScanFilter object if it was previously obtained.</param>
    /// <param name="scanStatistics">Optionally provide a ScanStatistics object if it was previously obtained.</param>
    private void ProcessSpectrumInformation(IScanFilter? scanFilter = null, ScanStatistics? scanStatistics = null, bool ext = false)
    {
      //If scanFilter or scanStatistics was not provided, grab them now.
      if (scanFilter == null) scanFilter = RawFile.GetFilterForScanNumber(CurrentScanNumber);
      if (scanStatistics == null) scanStatistics = RawFile.GetScanStatsForScanNumber(CurrentScanNumber);

      //Add spectrum information or call functions to process the information already obtained
      if (ext)
      {
        spectrumEx.RetentionTime = RawFile.RetentionTimeFromScanNumber(CurrentScanNumber);
        spectrumEx.ScanFilter = RawFile.GetFilterForScanNumber(CurrentScanNumber).ToString();  //TODO: consider processing the ScanFilter
      }
      else
      {
        spectrum.RetentionTime = RawFile.RetentionTimeFromScanNumber(CurrentScanNumber);
        spectrum.ScanFilter = RawFile.GetFilterForScanNumber(CurrentScanNumber).ToString();  //TODO: consider processing the ScanFilter
      }
      spectrum.RetentionTime = RawFile.RetentionTimeFromScanNumber(CurrentScanNumber);
      spectrum.ScanFilter = RawFile.GetFilterForScanNumber(CurrentScanNumber).ToString();  //TODO: consider processing the ScanFilter
      ProcessSpectrumStatistics(scanStatistics, ext);
      ProcessSpectrumFilter(scanFilter, ext);

      if (scanFilter.MSOrder != MSOrderType.Ms)
      {
        IScanEvent scanEvent = RawFile.GetScanEventForScanNumber(CurrentScanNumber);
        ProcessScanEvent(scanEvent, ext);
      }

      ILogEntryAccess trailerData = RawFile.GetTrailerExtraInformation(CurrentScanNumber);
      ProcessTrailerExtraInformation(trailerData, ext);
    }

    /// <summary>
    /// Used to process precursor ion information
    /// </summary>
    /// <param name="scanEvent">IScanEvent object</param>
    private void ProcessScanEvent(IScanEvent scanEvent, bool ext = false)
    {
      //Get all the precursor information
      IReaction reaction = scanEvent.GetReaction(0);
      PrecursorIon pre = new PrecursorIon();
      pre.IsolationMz = reaction.PrecursorMass;
      pre.IsolationWidth = reaction.IsolationWidth;
      if (ext) spectrumEx.Precursors.Add(pre);
      else spectrum.Precursors.Add(pre);
    }

    /// <summary>
    /// Process the scan filter for a spectrum. Provides useful header information.
    /// </summary>
    /// <param name="filter">IScanFilter object</param>
    private void ProcessSpectrumFilter(IScanFilter filter, bool ext = false)
    {
      if (ext)
      {
        spectrumEx.MsLevel = (int)filter.MSOrder;
        if (filter.Polarity == PolarityType.Positive) spectrumEx.Polarity = true;
        else spectrumEx.Polarity = false;

        switch (filter.MassAnalyzer)
        {
          case MassAnalyzerType.MassAnalyzerFTMS:
            spectrumEx.Analyzer = "FTMS";
            break;
          case MassAnalyzerType.MassAnalyzerITMS:
            spectrumEx.Analyzer = "ITMS";
            break;
          case MassAnalyzerType.MassAnalyzerASTMS:
            spectrumEx.Analyzer = "Astral";
            break;
          default:
            spectrumEx.Analyzer = "Unknown";
            break;
        }

        spectrumEx.ScanType = filter.ScanMode.ToString();
      }
      else
      {
        spectrum.MsLevel = (int)filter.MSOrder;
        if (filter.Polarity == PolarityType.Positive) spectrum.Polarity = true;
        else spectrum.Polarity = false;

        switch (filter.MassAnalyzer)
        {
          case MassAnalyzerType.MassAnalyzerFTMS:
            spectrum.Analyzer = "FTMS";
            break;
          case MassAnalyzerType.MassAnalyzerITMS:
            spectrum.Analyzer = "ITMS";
            break;
          case MassAnalyzerType.MassAnalyzerASTMS:
            spectrum.Analyzer = "Astral";
            break;
          default:
            spectrum.Analyzer = "Unknown";
            break;
        }

        spectrum.ScanType = filter.ScanMode.ToString();
      }

    }

    /// <summary>
    /// Process the scan statistics for a spectrum. Provides useful header information.
    /// </summary>
    /// <param name="scanStatistics">ScanStatistics object</param>
    private void ProcessSpectrumStatistics(ScanStatistics scanStatistics, bool ext = false)
    {
      if (ext)
      {
        spectrumEx.ScanNumber = scanStatistics.ScanNumber;
        spectrumEx.Centroid = scanStatistics.IsCentroidScan;
        spectrumEx.TotalIonCurrent = scanStatistics.TIC;
        spectrumEx.BasePeakMz = scanStatistics.BasePeakMass;
        spectrumEx.BasePeakIntensity = scanStatistics.BasePeakIntensity;
        spectrumEx.StartMz = scanStatistics.LowMass;
        spectrumEx.EndMz = scanStatistics.HighMass;
      }
      else
      {
        spectrum.ScanNumber = scanStatistics.ScanNumber;
        spectrum.Centroid = scanStatistics.IsCentroidScan;
        spectrum.TotalIonCurrent = scanStatistics.TIC;
        spectrum.BasePeakMz = scanStatistics.BasePeakMass;
        spectrum.BasePeakIntensity = scanStatistics.BasePeakIntensity;
        spectrum.StartMz = scanStatistics.LowMass;
        spectrum.EndMz = scanStatistics.HighMass;
      }
    }

    //TODO: Reassess these values closely. Some trailer information applies to multiple precursors. Other trailer information to only the first precursor.
    /// <summary>
    /// Processes the Trailer Extra Information attached to the Scan Header.
    /// </summary>
    /// <param name="trailerData">ILogEntryAccess object</param>
    private void ProcessTrailerExtraInformation(ILogEntryAccess trailerData, bool ext = false)
    {

      for (int i = 0; i < trailerData.Length; i++)
      {
        //for diagnostics, to see all trailer values
        //Console.WriteLine("TD: " + trailerData.Labels[i] + " = " + trailerData.Values[i]); 

        switch (MetaDictionary.FindMeta(trailerData.Labels[i]))
        {
          case MetaClass.ChargeState:
            if (ext)
            {
              if (spectrumEx.Precursors.Count > 0) spectrumEx.Precursors[0].Charge = Convert.ToInt32(trailerData.Values[i]);
            }
            else
            {
              if (spectrum.Precursors.Count > 0) spectrum.Precursors[0].Charge = Convert.ToInt32(trailerData.Values[i]);
            }
            break;
          case MetaClass.FaimsCV:
            if (ext) spectrumEx.FaimsCV = Convert.ToDouble(trailerData.Values[i]);
            else spectrum.FaimsCV = Convert.ToDouble(trailerData.Values[i]);
            break;
          case MetaClass.FaimsState:
            if (ext)
            {
              if (trailerData.Values[i] == "true" || trailerData.Values[i] == "True") spectrumEx.FaimsState = true;
              else spectrumEx.FaimsState = false;
            }
            else
            {
              if (trailerData.Values[i] == "true" || trailerData.Values[i] == "True") spectrum.FaimsState = true;
              else spectrum.FaimsState = false;
            }
            break;
          case MetaClass.IIT:
            if (ext) spectrumEx.IonInjectionTime = Convert.ToDouble(trailerData.Values[i]);
            else spectrum.IonInjectionTime = Convert.ToDouble(trailerData.Values[i]);
            break;
          case MetaClass.MonoisotopicMZ:
            if (ext)
            {
              if (spectrumEx.Precursors.Count > 0) spectrumEx.Precursors[0].MonoisotopicMz = Convert.ToDouble(trailerData.Values[i]);
            }
            else
            {
              if (spectrum.Precursors.Count > 0) spectrum.Precursors[0].MonoisotopicMz = Convert.ToDouble(trailerData.Values[i]);
            }
            break;
          default:

            //TODO: comment this out to disable notifications. But also maybe consider if any of these additional values are
            //worth capturing.
            //Console.WriteLine("Uncaptured trailerData: '" + trailerData.Labels[i] + "' " + trailerData.Values[i]);
            break;
        }

        //switch (trailerData.Labels[i])
        //{
        //  case "Charge State:":
        //    spectrum.Precursors[0].Charge = Convert.ToInt32(trailerData.Values[i]);
        //    break;
        //  case "Ion Injection Time (ms):":
        //    spectrum.IonInjectionTime = Convert.ToDouble(trailerData.Values[i]);
        //    break;
        //  case "Master Index:":
        //  case "Master Scan Number:":
        //  case "Master Scan Number":
        //    spectrum.PrecursorMasterScanNumber = Convert.ToInt32(trailerData.Values[i]);
        //    break;
        //  case "Monoisotopic M/Z:":
        //    spectrum.Precursors[0].MonoisotopicMz = Convert.ToDouble(trailerData.Values[i]);
        //    break;
        //  default:
        //    break;
        //}
      }
    }

    public void Reset()
    {
      CurrentScanNumber = 0;
    }

  }
}

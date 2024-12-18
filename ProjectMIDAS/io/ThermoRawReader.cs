using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader;

using ProjectMIDAS.Data.Spectrum;
using System.Collections.Specialized;
using System.Formats.Tar;
using ThermoFisher.CommonCore.Data.FilterEnums;
using ProjectMIDAS.IO;

namespace ProjectMIDAS.io
{
  internal class ThermoRawReader : ISpectrumFileReader
  {

    private Spectrum spectrum;
    private SpectrumEx spectrumEx;

    private IRawDataPlus? RawFile;

    private MSFilter Filter { get; set; } 

    private int lastScanNumber { get; set; } = 0;
    private int CurrentScanNumber = 0;

    public ThermoRawReader(MSFilter filter)
    {
      Filter = filter;
      spectrum = new Spectrum();
      spectrumEx = new SpectrumEx();
    }

    public Spectrum GetSpectrum(int scanNumber, bool centroid)
    {
      //Set the scan number, or if one wasn't specified (i.e. -1), then go to the next scan.
      if (scanNumber < 0) CurrentScanNumber++;
      else CurrentScanNumber = scanNumber;
      if (CurrentScanNumber > lastScanNumber)
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
            if (CurrentScanNumber > lastScanNumber)
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

      //check scan type. If profile, and user wants centroid, see if centroid version is also available.
      if (centroid)
      {

        //Regardless of actual scan peak type, see if it contains centroid peak data.
        bool centroidOK = false;
        if (scanStatistics.IsCentroidScan) centroidOK = true;
        else if (scanStatistics.SpectrumPacketType == SpectrumPacketType.FtProfile) centroidOK = true;

        //Get the centroid peaks
        if(centroidOK)
        {
          var centroidStream = RawFile.GetCentroidStream(CurrentScanNumber, false);
          spectrum = new Spectrum(centroidStream.Length);
          for(int i = 0; i < centroidStream.Length; i++)
          {
            spectrum.DataPoints[i].Mz = centroidStream.Masses[i];
            spectrum.DataPoints[i].Intensity = centroidStream.Intensities[i];
          }
          ProcessSpectrumStatistics(scanStatistics);
          ProcessSpectrumFilter(scanFilter);
          return spectrum;
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
      ProcessSpectrumStatistics(scanStatistics);
      ProcessSpectrumFilter(scanFilter);
      return spectrum;
    }

    public bool Open(string fileName)
    {
      RawFile = RawFileReaderAdapter.FileFactory(fileName);
      if (!RawFile.IsOpen) return false;
      RawFile.SelectInstrument(Device.MS, 1);
      CurrentScanNumber = 0;
      lastScanNumber = RawFile.RunHeaderEx.LastSpectrum;
      return true;
    }

    public void Close()
    {
      if(RawFile != null) RawFile.Dispose();
    }

    /// <summary>
    /// Process the scan filter for a spectrum. Provides useful header information.
    /// </summary>
    /// <param name="filter"></param>
    void ProcessSpectrumFilter(IScanFilter filter)
    {
      spectrum.MsLevel = (int)filter.MSOrder;
      if(filter.Polarity==PolarityType.Positive) spectrum.Polarity = true;
      else spectrum.Polarity = false;

    }

    /// <summary>
    /// Process the scan statistics for a spectrum. Provides useful header information.
    /// </summary>
    /// <param name="scanStatistics"></param>
    void ProcessSpectrumStatistics(ScanStatistics scanStatistics)
    {
      spectrum.ScanNumber = scanStatistics.ScanNumber;
    }

  }
}

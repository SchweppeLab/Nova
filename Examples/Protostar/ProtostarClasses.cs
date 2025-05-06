using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.Logging;
using Nova.Data;
using Nova.Io.Read;
using Nova.Io;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data;

namespace Protostar
{
  internal struct msPoint
  {
    public double Mz;
    public float Intensity;
    public msPoint(double mz=0, float intensity = 0)
    {
      Mz = mz;
      Intensity = intensity;
    }
  }
  internal class mPreprocessStruct
  { //adapted from Comet
    public int iMaxXCorrArraySize=0;
    public int iHighestIon =0;
    public double dHighestIntensity =0;
    public msPoint[] pdCorrelationData;
  }

  internal class ProtostarFile
  {
    public string Filename= string.Empty;
    public SpectrumEx? Scan;
    public Spectrum? Chromatogram;
    public int ScanCount = 0;

    public bool IsLoaded { get; private set; } = false;

    private FileReader MSReader = new FileReader();

    public event EventHandler? StartChromatogram;
    public event EventHandler? StopChromatogram;

    public ProtostarFile(string filename)
    {
      Filename = filename;
    }

    public void DoneLoading()
    {
      IsLoaded = true;
    }

    public void GenerateChromatogram()
    {
      StartChromatogram?.Invoke(this, new EventArgs());
      Spectrum spec = MSReader.ReadSpectrum(Filename);
      Chromatogram = new Spectrum(MSReader.ScanCount);
      ScanCount = MSReader.ScanCount;
      int i = 0;
      while (spec.ScanNumber > 0)
      {
        Chromatogram.DataPoints[i].Mz = spec.RetentionTime;
        Chromatogram.DataPoints[i++].Intensity = spec.TotalIonCurrent;
        spec = MSReader.ReadSpectrum();
      }
      
      MSReader.Reset();
      Scan = MSReader.ReadSpectrumEx(Filename);
      StopChromatogram?.Invoke(this,new EventArgs());
    }

    public void NextScan(int scanNum=-1)
    {
      SpectrumEx spectrum;
      if (scanNum < 0) spectrum = MSReader.ReadSpectrumEx();
      else spectrum = MSReader.ReadSpectrumEx(Filename, scanNum);
      if(spectrum.ScanNumber>0) Scan=spectrum;
    }

    public void PrevScan()
    {
      if (Scan == null) return;
      int prevScan = Scan.ScanNumber - 1;
      while (prevScan > 0)
      {
        SpectrumEx spectrum = MSReader.ReadSpectrumEx(Filename, prevScan);
        if (spectrum.ScanNumber > 0)
        {
          Scan = spectrum;
          return;
        }
        prevScan--;
      }
    }

    public double XCorr(List<double> peaks)
    {
      if(Scan == null) return 0;
      if (Scan.MsLevel != 2) return 0;

      double xcorr = 0;
      double binSize = 0.02;
      double binOffset = 0.0;

      //Convert current spectrum to binned array
      if (Scan.Analyzer.Contains("IT"))
      {
        binSize = 1.0005;
        binOffset = 0.4;
      }
      //Max pep size is 4000 Da
      int xCorrArraySize = (int)((4000.0 + 100.0) / binSize);

      mPreprocessStruct pPre = new mPreprocessStruct();
      pPre.iMaxXCorrArraySize = xCorrArraySize;
      xCorrArraySize = BinIons(pPre,binSize,binOffset);

      double[] pdTempRawData = new double[xCorrArraySize];
      double[] pdTmpFastXcorrData = new double[xCorrArraySize];
      float[] pfFastXcorrData = new float[xCorrArraySize];

      MakeCorrData(ref pdTempRawData, pPre, 50.0);

      // Make fast xcorr spectrum.
      double dm = 1.0 / 150;
      double dSum = 0.0;
      for (int i = 0; i < 75; i++) dSum += pPre.pdCorrelationData[i].Intensity;
      for (int i = 75; i < xCorrArraySize + 75; i++)
      {
        if (i < xCorrArraySize && pPre.pdCorrelationData[i].Intensity > 0) dSum += pPre.pdCorrelationData[i].Intensity;
        if (i >= 151 && pPre.pdCorrelationData[i - 151].Intensity > 0) dSum -= pPre.pdCorrelationData[i - 151].Intensity;
        pdTmpFastXcorrData[i - 75] = (dSum - pPre.pdCorrelationData[i - 75].Intensity) * dm;
      }

      pfFastXcorrData[0] = 0;
      for (int i = 1; i < pPre.iMaxXCorrArraySize - 1; i++)
      {
        double dTmp = pPre.pdCorrelationData[i].Intensity - pdTmpFastXcorrData[i];
        pfFastXcorrData[i] = (float)dTmp;

        // Allow user to set flanking peaks
        if (true)
        {
          int iTmp = i - 1;
          pfFastXcorrData[i] += (float)((pPre.pdCorrelationData[iTmp].Intensity - pdTmpFastXcorrData[iTmp]) * 0.5);

          iTmp = i + 1;
          pfFastXcorrData[i] += (float)((pPre.pdCorrelationData[iTmp].Intensity - pdTmpFastXcorrData[iTmp]) * 0.5);
        }
      }      

      //Score the peaks
      foreach(double d in peaks)
      {
        int index = (int)(d/binSize+(1-binOffset));
        if (index < pfFastXcorrData.Length) xcorr += pfFastXcorrData[index];
      }
      xcorr *= 0.005;

      //Return score
      return xcorr;
    }

    private int BinIons(mPreprocessStruct pPre,double binSize, double binOffset)
    {
      int i;
      int j;
      double dPrecursor=0;
      double dIon;
      double dIntensity;
      double invBinSize = 1/binSize;

      // Just need to pad iArraySize by 75.
      foreach(PrecursorIon p in Scan.Precursors) {
        int z = p.Charge;
        if (z == 0) z = 4;
        if (p.MonoisotopicMz == 0)
        {
          if(p.IsolationMz > dPrecursor) dPrecursor = p.IsolationMz*z;
        } else
        {
          if (p.MonoisotopicMz > dPrecursor) dPrecursor = p.IsolationMz*z;
        }
      }
      int xCorrArraySize = (int)((dPrecursor + 100.0) / binSize);
      if (xCorrArraySize > pPre.iMaxXCorrArraySize) xCorrArraySize = pPre.iMaxXCorrArraySize;
      pPre.pdCorrelationData = new msPoint[xCorrArraySize];
      pPre.iMaxXCorrArraySize = xCorrArraySize;

      foreach(SpecDataPointEx c in Scan.DataPoints) {
        dIon = c.Mz;
        dIntensity = c.Intensity;

        if (dIntensity > 0.0)
        {
          if (dIon < (dPrecursor + 50.0))
          {
            int iBinIon = (int)(dIon * invBinSize + (1-binOffset));
            dIntensity = Math.Sqrt(dIntensity);

            if (iBinIon > pPre.iHighestIon) pPre.iHighestIon = iBinIon;

            if ((iBinIon < xCorrArraySize) && (dIntensity > pPre.pdCorrelationData[iBinIon].Intensity))
            {
              if (dIntensity > pPre.pdCorrelationData[iBinIon].Mz)
              {
                pPre.pdCorrelationData[iBinIon].Intensity = (float)dIntensity;
                pPre.pdCorrelationData[iBinIon].Mz = dIon;
              }
              if (pPre.pdCorrelationData[iBinIon].Intensity > pPre.dHighestIntensity) pPre.dHighestIntensity = pPre.pdCorrelationData[iBinIon].Intensity;
            }
          }
        }
      }
      return xCorrArraySize;
    }

    void MakeCorrData(ref double[] pdTempRawData, mPreprocessStruct pPre, double scale)
    {
      int i;
      int ii;
      int iBin;
      int iWindowSize;
      int iNumWindows = 10;
      double dMaxWindowInten;
      double dMaxOverallInten;
      double dTmp1;
      double dTmp2;

      dMaxOverallInten = 0.0;

      // Normalize maximum intensity to 100.
      dTmp1 = 1.0;
      if (pPre.dHighestIntensity > 0.000001) dTmp1 = 100.0 / pPre.dHighestIntensity;

      for (i = 0; i < pPre.iMaxXCorrArraySize; i++)
      {
        pdTempRawData[i] = pPre.pdCorrelationData[i].Intensity * dTmp1;
        pPre.pdCorrelationData[i].Intensity = 0;
        if (dMaxOverallInten < pdTempRawData[i]) dMaxOverallInten = pdTempRawData[i];
      }

      iWindowSize = (int)Math.Ceiling((double)(pPre.iHighestIon) / iNumWindows);

      for (i = 0; i < iNumWindows; i++)
      {
        dMaxWindowInten = 0.0;
        for (ii = 0; ii < iWindowSize; ii++)
        {   // Find max inten. in window.
          iBin = i * iWindowSize + ii;
          if (iBin < pPre.iMaxXCorrArraySize)
          {
            if (pdTempRawData[iBin] > dMaxWindowInten) dMaxWindowInten = pdTempRawData[iBin];
          }
        }

        if (dMaxWindowInten > 0.0)
        {
          dTmp1 = scale / dMaxWindowInten;
          dTmp2 = 0.05 * dMaxOverallInten;

          for (ii = 0; ii < iWindowSize; ii++)
          {    // Normalize to max inten. in window.      
            iBin = i * iWindowSize + ii;
            if (iBin < pPre.iMaxXCorrArraySize)
            {
              if (pdTempRawData[iBin] > dTmp2) pPre.pdCorrelationData[iBin].Intensity = (float)(pdTempRawData[iBin] * dTmp1);
            }
          }
        }
      }

    }

    public override string ToString()
    {
      return Filename.Substring(Filename.LastIndexOf('\\')+1);
    }
  }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nova.Data.Spectrum;

namespace Nova.Io
{
  public interface ISpectrumFileReader : IEnumerable
  {
    //TODO: decide if we want to have general access to the last spectrum loaded
    //Spectrum Spectrum;
    //SpectrumEx SpectrumEx;

    /// <summary>
    /// Reads a spectrum from the currently open MS data file. 
    /// </summary>
    /// <param name="scanNumber">Optional scan number to grab. If omitted (or negative), the next spectrum in the file is loaded</param>
    /// <param name="centroid">Indicates the preferred peak type. Note that this preference is not guaranteed and should be checked 
    /// using the Spectrum.Centroid property. </param>
    /// <returns>Spectrum object</returns>
    Spectrum GetSpectrum(int scanNumber=-1, bool centroid=true);

    /// <summary>
    /// Open data file and hold new scan information.
    /// </summary>
    /// <param name="fileName">Path to the data file.</param>
    bool Open(string fileName);

    //TODO: decide if this is worthwhile
    /// <summary>
    /// Return some metadata about the file.
    /// </summary>
    /// <returns>Returns an instance of ScanFileHeader</returns>
    //ScanFileHeader GetHeader();

    /// <summary>
    /// Performs any cleanup.
    /// </summary>
    void Close();
  }
}

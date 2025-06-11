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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nova.Data;

namespace Nova.Io.Read
{
  public interface ISpectrumFileReader : IEnumerable
  {
    //TODO: decide if we want to have general access to the last spectrum loaded
    //Spectrum Spectrum;
    //SpectrumEx SpectrumEx;

    MSFilter Filter { get; set; }

    int ScanCount { get; }

    int FirstScan { get; }
    int LastScan { get; }
    double MaxRetentionTime { get; } 

    /// <summary>
    /// Reads a chromatogram from the currently open MS data file
    /// </summary>
    /// <param name="chromatIndex">Optional chromatogram index (zero based) to read. If omitted (or negative), the next chromatogram in the file is read.</param>
    /// <returns>Chromatogram object</returns>
    Chromatogram GetChromatogram(int chromatIndex = -1);

    /// <summary>
    /// Reads a spectrum from the currently open MS data file. 
    /// </summary>
    /// <param name="scanNumber">Optional scan number to grab. If omitted (or negative), the next spectrum in the file is loaded</param>
    /// <param name="centroid">Indicates the preferred peak type. Note that this preference is not guaranteed and should be checked 
    /// using the Spectrum.Centroid property. </param>
    /// <returns>Spectrum object</returns>
    Spectrum GetSpectrum(int scanNumber = -1, bool centroid = true);

    SpectrumEx GetSpectrumEx(int scanNumber = -1, bool centroid = true);

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

    /// <summary>
    /// Resets the file reader to the beginning of the file when iteratively reading.
    /// </summary>
    void Reset();
  }
}

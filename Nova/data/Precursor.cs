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

namespace Nova.Data
{
  /// <summary>
  /// The precursor ion of a dependent scan and how it was isolated and fragmented
  /// </summary>
  public class PrecursorIon
  {
    /// <summary>
    /// PrecursorIon object constructor
    /// </summary>
    public PrecursorIon()
    {
    }

    /// <summary>
    /// PrecursorIon object copy constructor
    /// </summary>
    /// <param name="pi">The precursor to copy</param>
    public PrecursorIon(PrecursorIon pi)
    {
      MonoisotopicMz=pi.MonoisotopicMz;
      Intensity=pi.Intensity;
      Charge=pi.Charge;
      IsolationMz=pi.IsolationMz;
      IsolationWidth=pi.IsolationWidth;
      CollisionEnergy = pi.CollisionEnergy;
      FragmentationMethod = pi.FragmentationMethod;
      IsolationSpecificity = pi.IsolationSpecificity;
    }

    /// <summary>
    /// Creates a precursor from its monoisotopic m/z, with optional intensity, charge, isolation m/z, and isolation width
    /// </summary>
    /// <param name="mz">The monoisotopic m/z</param>
    /// <param name="intensity">The intensity of the precursor peak</param>
    /// <param name="charge">The charge state</param>
    /// <param name="isoMz">The isolation m/z of the precursor selection window</param>
    /// <param name="isoWidth">The isolation width of the precursor selection window</param>
    public PrecursorIon(double mz, double intensity = 0, int charge = 0, double isoMz=0, double isoWidth=0)
    {
      MonoisotopicMz = mz;
      Intensity = intensity;
      Charge = charge;
      IsolationMz = isoMz;
      IsolationWidth = isoWidth;
    }

    /// <summary>
    /// Resets all variables to 0.
    /// </summary>
    public void Clear()
    {
      MonoisotopicMz = 0;
      Intensity = 0;
      Charge = 0;
      IsolationMz = 0;
      IsolationWidth = 0;
      CollisionEnergy = 0;
      FragmentationMethod = FragmentationType.None;
      IsolationSpecificity = 0;
    }

    /// <summary>
    /// The putative monoisotopic m/z peak of the isotope envelope that contains the isolation m/z.
    /// </summary>
    public double MonoisotopicMz { get; set; } = 0;

    /// <summary>
    /// Intensity of the IsolationMz precursor peak.
    /// </summary>
    public double Intensity { get; set; } = 0;

    /// <summary>
    /// Charge state of the precursor.
    /// </summary>
    public int Charge { get; set; } = 0;

    /// <summary>
    /// If a dependent scan, the fragmentation energy used
    /// </summary>
    public double CollisionEnergy { get; set; } = 0;

    /// <summary>
    /// The fragmentation method used, or <see cref="FragmentationType.None"/> if not known
    /// </summary>
    public FragmentationType FragmentationMethod { get; set; } = FragmentationType.None;

    /// <summary>
    /// The m/z that the instrument targeted for isolation.
    /// </summary>
    public double IsolationMz { get; set; } = 0;

    //TODO: Update this to allow for asymmetric isolation windows.
    /// <summary>
    /// The size of the window that the instrument targeted for isolation.
    /// </summary>
    public double IsolationWidth { get; set; } = 0;

    //TODO: Determine if this is relevant; might be Monocle-specific...
    /// <summary>
    /// Proportion of the intensity in the isolation window
    /// that belongs to the precursor.
    /// 
    /// This should be a value from zero to one.
    /// </summary>
    public double IsolationSpecificity { get; set; }
  }
}

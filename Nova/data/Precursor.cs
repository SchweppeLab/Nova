﻿// Copyright 2025 Michael Hoopmann
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Data
{
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
    /// <param name="pi"></param>
    public PrecursorIon(PrecursorIon pi)
    {
      MonoisotopicMz=pi.MonoisotopicMz;
      Intensity=pi.Intensity;
      Charge=pi.Charge;
      IsolationMz=pi.IsolationMz;
      IsolationWidth=pi.IsolationWidth;
    }

    public PrecursorIon(double mz, double intensity = 0, int charge = 0)
    {
      MonoisotopicMz = mz;
      Intensity = intensity;
      Charge = charge;
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

    public FramentationType FramentationMethod { get; set; } = FramentationType.None;

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

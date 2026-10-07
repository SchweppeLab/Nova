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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nova.Data
{
  /// <summary>
  /// The common contract for a single data point in a mass spectrum, minimally an m/z and intensity pair. 
  /// <see cref="TSpectrum{T}"/> is written against this interface so one spectrum implementation
  /// serves both <see cref="SpecDataPoint"/> and <see cref="SpecDataPointEx"/>
  /// </summary>
  public interface ISpecDataPoint
  {
    /// <summary>
    /// The mass to charge ratio (m/z) value for a mass spectrum data point
    /// </summary>
    double Mz { get; set; }

    /// <summary>
    /// The intensity value for a mass spectrum data point
    /// </summary>
    double Intensity { get; set; }

    /// <summary>
    /// Populates the data point from its binary representation, as written by <see cref="Write"/>
    /// </summary>
    /// <param name="reader">A reader positioned at the start of this data point's values</param>
    void Read(BinaryReader reader);

    /// <summary>
    /// Serializes the data point to its binary representation, as read back by <see cref="Read"/>
    /// </summary>
    /// <param name="writer">A writer positioned where this data point's values should go</param>
    void Write(BinaryWriter writer);
  }

  /// <summary>
  /// The basic mass spectrum data point: an m/z and intensity pair. This is the point type of <see cref="Spectrum"/>
  /// </summary>
  public struct SpecDataPoint : ISpecDataPoint, IComparable<SpecDataPoint>
  {
    /// <summary>
    /// The mass to charge ratio (m/z) value for this data point
    /// </summary>
    public double Mz { get; set; }

    /// <summary>
    /// The intensity value for this data point
    /// </summary>
    public double Intensity { get; set; }

    /// <summary>
    /// Creates a data point, with both values defaulting to zero
    /// </summary>
    /// <param name="mz">The m/z value</param>
    /// <param name="intensity">The intensity value</param>
    public SpecDataPoint(double mz=0, double intensity = 0)
    {
      Mz = mz;
      Intensity = intensity;
    }

    /// <summary>
    /// Reads the m/z then the intensity from the stream, each as a double
    /// </summary>
    /// <param name="reader">A reader positioned at the start of this data point's values</param>
    public void Read(BinaryReader reader)
    {
      Mz = reader.ReadDouble();
      Intensity = reader.ReadDouble();
    }

    /// <summary>
    /// Writes the m/z then the intensity to the stream, each as a double
    /// </summary>
    /// <param name="writer">A writer positioned where this data point's values should go</param>
    public void Write(BinaryWriter writer)
    {
      writer.Write(Mz);
      writer.Write(Intensity);
    }

    /// <summary>
    /// Orders data points by m/z alone. Two points at the same m/z compare as equal
    /// </summary>
    /// <param name="x">The data point to compare against</param>
    /// <returns>Negative, zero or positive as this point's m/z is less than, equal to or greater than x's</returns>
    public int CompareTo(SpecDataPoint x)
    {
      return Mz.CompareTo(x.Mz);
    }
  }

  /// <summary>
  /// An extended mass spectrum data point: m/z and intensity plus the per-peak attributes a Thermo centroid stream
  /// provides. This is the point type within <see cref="SpectrumEx"/>. Only the Thermo RAW reader fills the extra fields;
  /// the mzML, mzXML and MGF readers set m/z and intensity and leave the rest at zero
  /// </summary>
  public struct SpecDataPointEx : ISpecDataPoint, IComparable<SpecDataPointEx>
  {
    /// <summary>
    /// The mass to charge ratio (m/z) value for this data point
    /// </summary>
    public double Mz { get; set; }

    /// <summary>
    /// The intensity value for this data point
    /// </summary>
    public double Intensity { get; set; }

    /// <summary>
    /// The noise level the instrument reported at this peak
    /// </summary>
    public double Noise { get; set; }

    /// <summary>
    /// The baseline level the instrument reported at this peak
    /// </summary>
    public double Baseline { get; set; }

    /// <summary>
    /// The charge state the instrument assigned to this peak, or zero if none was assigned
    /// </summary>
    public int Charge { get; set; }

    /// <summary>
    /// The mass resolution the instrument measured at this peak
    /// </summary>
    public double Resolution { get; set; }

    /// <summary>
    /// Creates a data point, with every value defaulting to zero
    /// </summary>
    /// <param name="mz">The m/z value</param>
    /// <param name="intensity">The intensity value</param>
    /// <param name="noise">The noise level at the peak</param>
    /// <param name="baseline">The baseline at the peak</param>
    /// <param name="charge">The assigned charge state</param>
    /// <param name="resolution">The resolution at the peak</param>
    public SpecDataPointEx(double mz = 0, double intensity = 0,double noise=0,double baseline=0,int charge=0, double resolution=0)
    {
      Mz = mz;
      Intensity = intensity;
      Noise = noise;
      Baseline = baseline;
      Charge = charge;
      Resolution = resolution;
    }

    /// <summary>
    /// Reads an extended mass spectrum data point
    /// </summary>
    /// <param name="reader">A reader positioned at the start of this data point's values</param>
    public void Read(BinaryReader reader)
    {
      Mz = reader.ReadDouble();
      Intensity = reader.ReadDouble();
      Noise = reader.ReadDouble();
      Baseline = reader.ReadDouble();
      Charge = reader.ReadInt32();
      Resolution = reader.ReadDouble();
    }

    /// <summary>
    /// Writes an extended mass spectrum data point
    /// </summary>
    /// <param name="writer">A writer positioned where this data point's values should go</param>
    public void Write(BinaryWriter writer)
    {
      writer.Write(Mz);
      writer.Write(Intensity);
      writer.Write(Noise);
      writer.Write(Baseline);
      writer.Write(Charge);
      writer.Write(Resolution);
    }

    /// <summary>
    /// Orders data points by m/z alone. Two points at the same m/z compare as equal
    /// </summary>
    /// <param name="x">The data point to compare against</param>
    /// <returns>Negative, zero or positive as this point's m/z is less than, equal to or greater than x's</returns>
    public int CompareTo(SpecDataPointEx x)
    {
      return Mz.CompareTo(x.Mz);
    }
  }

  
}

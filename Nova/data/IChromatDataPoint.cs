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
  /// The contract for a single data point in a chromatogram: a retention time and intensity pair
  /// </summary>
  public interface IChromatDataPoint
  {
    /// <summary>
    /// The retention time for a chromatogram data point. The unit is defined by the source, not here
    /// </summary>
    double RT { get; set; }

    /// <summary>
    /// The intensity value for a chromatogram data point
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
  /// The chromatogram data point: a retention time and intensity pair. This is the point type of <see cref="Chromatogram"/>
  /// </summary>
  public struct ChromatDataPoint : IChromatDataPoint
  {
    /// <summary>
    /// The retention time for this data point
    /// </summary>
    public double RT { get; set; }

    /// <summary>
    /// The intensity value for this data point
    /// </summary>
    public double Intensity { get; set; }

    /// <summary>
    /// Reads the retention time then the intensity from the stream, each as a double
    /// </summary>
    /// <param name="reader">A reader positioned at the start of this data point's values</param>
    public void Read(BinaryReader reader)
    {
      RT = reader.ReadDouble();
      Intensity = reader.ReadDouble();
    }

    /// <summary>
    /// Writes the retention time then the intensity to the stream, each as a double
    /// </summary>
    /// <param name="writer">A writer positioned where this data point's values should go</param>
    public void Write(BinaryWriter writer)
    {
      writer.Write(RT);
      writer.Write(Intensity);
    }
  }
}

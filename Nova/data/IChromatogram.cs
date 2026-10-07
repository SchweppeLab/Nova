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
  /// The contract for a chromatogram: a series of retention time and intensity data points
  /// </summary>
  public interface IChromatogram : IDisposable
  {
    /// <summary>
    /// The number of data points
    /// </summary>
    int Count { get; }

    /// <summary>
    /// The data points
    /// </summary>
    ChromatDataPoint[] DataPoints { get; set; }

    /// <summary>
    /// Replaces the contents with the data points from a <see cref="Serialize"/> payload
    /// </summary>
    /// <param name="data">A byte array produced by <see cref="Serialize"/></param>
    void Deserialize(byte[] data);

    /// <summary>
    /// Replaces the data points with a new, empty array of the given size. Existing points are discarded
    /// </summary>
    /// <param name="sz">The new number of data points</param>
    void Resize(int sz);

    /// <summary>
    /// Serializes the chromatogram to a byte array, suitable as a pipe message payload; read back by <see cref="Deserialize"/>
    /// </summary>
    /// <returns>The serialized chromatogram</returns>
    byte[] Serialize();
  }

  /// <summary>
  /// A chromatogram: a run of retention time and intensity data points, with the identifier the source gave it
  /// </summary>
  public class Chromatogram : IChromatogram
  {
    /// <summary>
    /// The identifier the source gave this chromatogram, such as "TIC". Not carried by <see cref="Serialize"/>
    /// </summary>
    public string ID { get; set; } = string.Empty;

    /// <summary>
    /// The number of data points
    /// </summary>
    public int Count { get; protected set; }

    /// <summary>
    /// The data points. Assigning a new array here directly does not update <see cref="Count"/>; use <see cref="Resize"/>
    /// </summary>
    public ChromatDataPoint[] DataPoints { get; set; }

    /// <summary>
    /// Creates a chromatogram sized for the given number of data points
    /// </summary>
    /// <param name="count">The number of data points</param>
    public Chromatogram(int count = 0)
    {
      Count = count;
      DataPoints = new ChromatDataPoint[count];
    }

    /// <summary>
    /// Reads the count as a 32-bit integer, then each data point via <see cref="ChromatDataPoint.Read"/>
    /// </summary>
    /// <param name="data">A byte array produced by <see cref="Serialize"/></param>
    public void Deserialize(byte[] data)
    {
      using (MemoryStream m = new MemoryStream(data))
      using (BinaryReader reader = new BinaryReader(m, System.Text.Encoding.Unicode))
      {
        Count = reader.ReadInt32();
        Resize(Count);
        for (int a = 0; a < Count; a++)
        {
          DataPoints[a].Read(reader);
        }

      }
    }

    /// <summary>
    /// Replaces the data points with a new, empty array of the given size. Existing points are discarded
    /// </summary>
    /// <param name="sz">The new number of data points</param>
    public void Resize(int sz)
    {
      Count = sz;
      DataPoints = new ChromatDataPoint[sz];
    }

    /// <summary>
    /// Writes the count as a 32-bit integer, then each data point via <see cref="ChromatDataPoint.Write"/>
    /// </summary>
    /// <returns>The serialized chromatogram</returns>
    public byte[] Serialize()
    {
      using (MemoryStream m = new MemoryStream())
      using (BinaryWriter writer = new BinaryWriter(m, System.Text.Encoding.Unicode))
      {
        writer.Write(Count);
        for (int a = 0; a < Count; a++)
        {
          DataPoints[a].Write(writer);
        }

        return m.ToArray();
      }
    }

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    /// <summary>
    /// Nothing to release in this class; a chromatogram holds no unmanaged resources. The hook is here for a subclass that does
    /// </summary>
    /// <param name="disposing">True when called from <see cref="Dispose()"/> rather than a finalizer</param>
    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
        }
        disposedValue = true;
      }
    }

    // This code added to correctly implement the disposable pattern.
    /// <summary>
    /// A no-op beyond marking the instance disposed. Present so a chromatogram can be used the same way as the spectrum
    /// types, which share this pattern
    /// </summary>
    public void Dispose()
    {
      // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
      Dispose(true);
    }
    #endregion
  }

}

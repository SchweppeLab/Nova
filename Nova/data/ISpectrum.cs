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
using System.IO;

namespace Nova.Data
{
  /// <summary>
  /// The contract for a mass spectrum as a series of data points of type <typeparamref name="T"/>
  /// </summary>
  /// <typeparam name="T">The data point type, <see cref="SpecDataPoint"/> or <see cref="SpecDataPointEx"/></typeparam>
  public interface ISpectrum<T> : IDisposable
  where T : ISpecDataPoint
  {
    /// <summary>
    /// The number of data points
    /// </summary>
    int Count { get; }

    /// <summary>
    /// The data points
    /// </summary>
    T[] DataPoints { get; set; }

    /// <summary>
    /// Loads the spectrum from a <see cref="Serialize"/> payload
    /// </summary>
    /// <param name="data">A byte array produced by <see cref="Serialize"/></param>
    void Deserialize(byte[] data);

    /// <summary>
    /// Finds the data point at an m/z, or the nearest one within a tolerance
    /// </summary>
    /// <param name="mz">The m/z to look for</param>
    /// <param name="ppm">Tolerance in parts per million; 0 means an exact match only</param>
    /// <returns>The index into <see cref="DataPoints"/>, or -1 if no point is within tolerance</returns>
    int GetMz(double mz, double ppm = 0);

    /// <summary>
    /// Replaces the data points with a new, empty array of the given size. Existing points are discarded
    /// </summary>
    /// <param name="sz">The new number of data points</param>
    void Resize(int sz);

    /// <summary>
    /// Serializes the spectrum to a byte array, suitable as a pipe message payload; read back by <see cref="Deserialize"/>
    /// </summary>
    /// <returns>The serialized spectrum</returns>
    byte[] Serialize();
  }

  /// <summary>
  /// The spectrum implementation shared by <see cref="Spectrum"/> and <see cref="SpectrumEx"/>: the scan-level fields of
  /// <see cref="SpectrumFoundation"/> plus the data points
  /// </summary>
  /// <typeparam name="T">The data point type</typeparam>
  public class TSpectrum<T> : SpectrumFoundation, ISpectrum<T>
  where T : ISpecDataPoint, new()
  {
    /// <summary>
    /// The data points. Assigning a new array here directly does not update <see cref="Count"/>; use <see cref="Resize"/>
    /// </summary>
    public T[] DataPoints { get; set; }

    /// <summary>
    /// The number of data points. Stored rather than computed from DataPoints.Length, for speed
    /// </summary>
    public int Count { get; protected set; }

    /// <summary>
    /// Creates a spectrum sized for the given number of data points
    /// </summary>
    /// <param name="count">The number of data points</param>
    public TSpectrum(int count = 0): base()
    {
      Count = count;
      DataPoints = new T[count];
    }

    /// <summary>
    /// Reads a byte array into spectrum object data members.
    /// </summary>
    /// <param name="data">A byte array produced by <see cref="Serialize"/></param>
    public void Deserialize(byte[] data)
    {
      using (MemoryStream m = new MemoryStream(data))
      using (BinaryReader reader = new BinaryReader(m, System.Text.Encoding.Unicode))
      {
        ScanNumber = reader.ReadInt32();
        MsLevel = reader.ReadInt32();
        Centroid = reader.ReadBoolean();
        RetentionTime = reader.ReadDouble();
        StartMz = reader.ReadDouble();
        EndMz = reader.ReadDouble();
        TotalIonCurrent = reader.ReadDouble();
        BasePeakIntensity = reader.ReadDouble();
        FaimsState = reader.ReadBoolean();
        FaimsCV = reader.ReadDouble();
        Analyzer = reader.ReadString();
        IonInjectionTime = reader.ReadDouble();
        ScanType = reader.ReadString();
        PrecursorMasterScanNumber = reader.ReadInt32();
        MasterIndex = reader.ReadInt32();

        ScanFilter = reader.ReadString();
        ScanDescription = reader.ReadString();

        Precursors.Clear();
        int pre = reader.ReadInt32();
        for (int a = 0; a < pre; a++)
        {
          PrecursorIon pi = new PrecursorIon();
          pi.IsolationMz = reader.ReadDouble();
          pi.IsolationWidth = reader.ReadDouble();
          pi.MonoisotopicMz = reader.ReadDouble();
          pi.Charge = reader.ReadInt32();
          Precursors.Add(pi);
        }

        Count = reader.ReadInt32();
        Resize(Count);
        for (int a = 0; a < Count; a++)
        {
          DataPoints[a].Read(reader);
        }

      }
    }

    //This function currently uses System.Array.BinarySearch. But I wonder if it could be faster
    //by just implementing a binary search without having to instanciate those objects and structs (and IComparable<>).
    /// <summary>
    /// Finds the data point at an m/z, or the nearest one within a tolerance. Requires <see cref="DataPoints"/> sorted by
    /// ascending m/z, the order the point types' CompareTo defines. If both neighbors of a miss are within tolerance, the
    /// lower-m/z one is returned
    /// </summary>
    /// <param name="mz">The m/z to look for</param>
    /// <param name="ppm">Tolerance in parts per million; 0 means an exact match only</param>
    /// <returns>The index into <see cref="DataPoints"/>, or -1 if the spectrum is empty or no point is within tolerance</returns>
    public int GetMz(double mz, double ppm = 0)
    {
      if (Count == 0) return -1;
      T tmp = new T();
      tmp.Mz = mz;
      int index = System.Array.BinarySearch(DataPoints, tmp);

      if (index >= 0) return index;

      double tol = mz / 1e6 * ppm;
      double min = mz - tol;
      double max = mz + tol;

      index = ~index;
      int indexB = index - 1;

      //if we're past the end of the array
      if (index >= Count)
      {
        if (indexB >= 0 && DataPoints[indexB].Mz >= min && DataPoints[indexB].Mz <= max) return indexB;
        return -1;
      }

      //if we're before the beginning of the array
      if (index == 0)
      {
        if (DataPoints[index].Mz >= min && DataPoints[index].Mz <= max) return index;
        return -1;
      }

      //Check both closest points
      if (DataPoints[indexB].Mz >= min && DataPoints[indexB].Mz <= max) return indexB;
      if (DataPoints[index].Mz >= min && DataPoints[index].Mz <= max) return index;
      return -1;
    }

    /// <summary>
    /// Replaces the data points with a new, empty array of the given size. Existing points are discarded
    /// </summary>
    /// <param name="sz">The new number of data points</param>
    public void Resize(int sz)
    {
      Count = sz;
      DataPoints = new T[sz];
    }

    /// <summary>
    /// Writes spectrum object data members into a byte array. Not every member is carried.
    /// </summary>
    /// <returns>The serialized spectrum</returns>
    public byte[] Serialize()
    {
      using (MemoryStream m = new MemoryStream())
      using (BinaryWriter writer = new BinaryWriter(m, System.Text.Encoding.Unicode))
      {
        writer.Write(ScanNumber);
        writer.Write(MsLevel);
        writer.Write(Centroid);
        writer.Write(RetentionTime);
        writer.Write(StartMz);
        writer.Write(EndMz);
        writer.Write(TotalIonCurrent);
        writer.Write(BasePeakIntensity);
        writer.Write(FaimsState);
        writer.Write(FaimsCV);
        writer.Write(Analyzer);
        writer.Write(IonInjectionTime);
        writer.Write(ScanType);
        writer.Write(PrecursorMasterScanNumber);
        writer.Write(MasterIndex);

        writer.Write(ScanFilter);
        writer.Write(ScanDescription);

        writer.Write(Precursors.Count);
        for (int a = 0; a < Precursors.Count; a++)
        {
          writer.Write(Precursors[a].IsolationMz);
          writer.Write(Precursors[a].IsolationWidth);
          writer.Write(Precursors[a].MonoisotopicMz);
          writer.Write(Precursors[a].Charge);
        }

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
    /// Nothing to release here; the hook is for a subclass that holds resources
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
    /// A no-op beyond marking the instance disposed; a spectrum holds no unmanaged resources
    /// </summary>
    public void Dispose()
    {
      // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
      Dispose(true);
    }
    #endregion
  }

  /// <summary>
  /// A spectrum of basic m/z and intensity data points. A class rather than a using alias of <see cref="TSpectrum{T}"/>
  /// because an alias would not be visible to consumers of the assembly
  /// </summary>
  public class Spectrum : TSpectrum<SpecDataPoint>, ISpectrum<SpecDataPoint>
  {
    /// <summary>
    /// Creates a spectrum sized for the given number of data points
    /// </summary>
    /// <param name="count">The number of data points</param>
    public Spectrum(int count = 0) : base(count)
    {
    }
  }

  /// <summary>
  /// A spectrum of extended data points carrying the per-peak attributes a Thermo centroid stream provides
  /// </summary>
  public class SpectrumEx : TSpectrum<SpecDataPointEx>, ISpectrum<SpecDataPointEx>
  {
    /// <summary>
    /// Creates a spectrum sized for the given number of data points
    /// </summary>
    /// <param name="count">The number of data points</param>
    public SpectrumEx(int count = 0) : base (count)
    {
    }
  }
}

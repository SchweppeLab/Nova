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

//The hope is that one day we can move this library out of Framework and into
//something recent. And that then these aliases would work, rather than having
//to use polymorphism as is done down below.
//global using Spectrum = Nova.Data.TSpectrum<Nova.Data.SpecDataPoint>;
//global using SpectrumEx = Nova.Data.TSpectrum<Nova.Data.SpecDataPointEx>;

namespace Nova.Data
{ 
  public interface ISpectrum<T> : IDisposable
  where T : ISpecDataPoint
  {
    int Count { get; }
    T[] DataPoints { get; set; }

    void Deserialize(byte[] data);
    int GetMz(double mz, double ppm = 0);
    void Resize(int sz);
    byte[] Serialize();
  }

  public class TSpectrum<T> : SpectrumFoundation, ISpectrum<T>
  where T : ISpecDataPoint, new()
  {
    public T[] DataPoints { get; set; }
    public int Count { get; protected set; } //=> DataPoints.Length; //this lambda is slow.

    public TSpectrum(int count = 0): base()
    {
      Count = count;
      DataPoints = new T[count];
    }

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

      //if we're before the beginning of the array
      if (index == 0)
      {
        if (DataPoints[index].Mz >= min && DataPoints[index].Mz <= max) return index;
      }
      else
      {
        if (index >= Count)
        {
          if (indexB >= 0 && DataPoints[indexB].Mz >= min && DataPoints[indexB].Mz <= max) return indexB;
        }
        else
        {
          //Check both closest points
          if (DataPoints[indexB].Mz >= min && DataPoints[indexB].Mz <= max) return indexB;
          if (DataPoints[index].Mz >= min && DataPoints[index].Mz <= max) return index;
        }
      }
      return -1;
    }

    public void Resize(int sz)
    {
      Count = sz;
      DataPoints = new T[sz];
    }

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
    public void Dispose()
    {
      // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
      Dispose(true);
    }
    #endregion
  }

  public class Spectrum : TSpectrum<SpecDataPoint>, ISpectrum<SpecDataPoint>
  {
    public Spectrum(int count = 0) : base(count)
    {
    }
  }

  public class SpectrumEx : TSpectrum<SpecDataPointEx>, ISpectrum<SpecDataPointEx>
  {
    public SpectrumEx(int count = 0) : base (count)
    {
    }
  }
}

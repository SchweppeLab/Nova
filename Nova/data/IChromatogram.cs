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
  internal interface IChromatogram : IDisposable
  {
    int Count { get; }
    ChromatDataPoint[] DataPoints { get; set; }

    void Deserialize(byte[] data);
    void Resize(int sz);
    byte[] Serialize();
  }

  public class Chromatogram : IChromatogram
  {
    public string ID { get; set; } = string.Empty;

    public int Count { get; protected set; }

    public ChromatDataPoint[] DataPoints { get; set; }

    public Chromatogram(int count = 0)
    {
      Count = count;
      DataPoints = new ChromatDataPoint[count];
    }

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

    public void Resize(int sz)
    {
      Count = sz;
      DataPoints = new ChromatDataPoint[sz];
    }

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

}

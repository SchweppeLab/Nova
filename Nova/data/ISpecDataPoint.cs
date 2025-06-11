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
  public interface ISpecDataPoint
  {
    double Mz { get; set; }
    double Intensity { get; set; }
    void Read(BinaryReader reader);
    void Write(BinaryWriter writer);
  }

  public struct SpecDataPoint : ISpecDataPoint, IComparable<SpecDataPoint>
  {
    public double Mz { get; set; }
    public double Intensity { get; set; }

    public SpecDataPoint(double mz=0, double intensity = 0)
    {
      Mz = mz;
      Intensity = intensity;
    }

    public void Read(BinaryReader reader)
    {
      Mz = reader.ReadDouble();
      Intensity = reader.ReadDouble();
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(Mz);
      writer.Write(Intensity);
    }

    public int CompareTo(SpecDataPoint x)
    {
      return Mz.CompareTo(x.Mz);
    }
  }

  public struct SpecDataPointEx : ISpecDataPoint, IComparable<SpecDataPointEx>
  {
    public double Mz { get; set; }
    public double Intensity { get; set; }
    public double Noise { get; set; }
    public double Baseline { get; set; }
    public int Charge { get; set; }
    public double Resolution { get; set; }

    public SpecDataPointEx(double mz = 0, double intensity = 0,double noise=0,double baseline=0,int charge=0, double resolution=0)
    {
      Mz = mz;
      Intensity = intensity;
      Noise = noise;
      Baseline = baseline;
      Charge = charge;
      Resolution = resolution;
    }

    public void Read(BinaryReader reader)
    {
      Mz = reader.ReadDouble();
      Intensity = reader.ReadDouble();
      Noise = reader.ReadDouble();
      Baseline = reader.ReadDouble();
      Charge = reader.ReadInt32();
      Resolution = reader.ReadDouble();
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(Mz);
      writer.Write(Intensity);
      writer.Write(Noise);
      writer.Write(Baseline);
      writer.Write(Charge);
      writer.Write(Resolution);
    }

    public int CompareTo(SpecDataPointEx x)
    {
      return Mz.CompareTo(x.Mz);
    }
  }

  
}

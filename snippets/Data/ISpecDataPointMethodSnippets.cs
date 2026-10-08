// Generated from the Example sections of the method pages. Do not hand-edit;
// change the page and regenerate.

using Nova.Data;
using System.IO;
using System;

namespace NovaSnippets.Data
{
  internal static class ISpecDataPointMethodSnippets
  {
    internal static void ISpecDataPoint_Read()
    {
      // Works for either point type.
      static T ReadPoint<T>(BinaryReader reader) where T : ISpecDataPoint, new()
      {
        T point = new T();
        point.Read(reader);
        return point;
      }
    }

    internal static void ISpecDataPoint_Write()
    {
      // Works for either point type.
      static void WritePoints<T>(BinaryWriter writer, T[] points) where T : ISpecDataPoint
      {
        foreach (T point in points)
        {
          point.Write(writer);
        }
      }
    }
  }
}

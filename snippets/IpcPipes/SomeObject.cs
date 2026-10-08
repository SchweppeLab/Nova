// Stands in for the user-defined payload type the PipesClient and PipesServer
// examples send. Not part of Nova and not published.

using System;
using System.IO;
using System.Text;

namespace NovaSnippets.IpcPipes
{
  internal class SomeObject
  {
    public int intData = 1;
    public double doubleData = 2.5;
    public string strData = "three";

    public byte[] Serialize()
    {
      using (MemoryStream stream = new MemoryStream())
      using (BinaryWriter writer = new BinaryWriter(stream))
      {
        writer.Write(intData);
        writer.Write(doubleData);
        writer.Write(strData);
        return stream.ToArray();
      }
    }

    public void Deserialize(byte[] data)
    {
      using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
      {
        intData = reader.ReadInt32();
        doubleData = reader.ReadDouble();
        strData = reader.ReadString();
      }
    }
  }
}

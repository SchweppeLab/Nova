using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectMIDAS.data
{
  public interface IMIDASSpectrum : IDisposable
  {
    public Dictionary<string,string> MetaData { get; }

    public void Deserialize(byte[] data);
    public byte[] Serialize();
  }
}
